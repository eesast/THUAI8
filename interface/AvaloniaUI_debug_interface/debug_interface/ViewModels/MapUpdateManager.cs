// ViewModels\MapUpdateManager.cs - 基于坐标的修复版本，完全替换原文件
using System;
using System.Collections.Generic;
using System.Linq;
using Protobuf;
using installer.Model;
using debug_interface.Models;

namespace debug_interface.ViewModels
{
    public delegate void BuildingEventHandler(string message, string level = "INFO");
    public class MapUpdateManager
    {
        // 改为使用坐标作为键的缓存
        private Dictionary<(int x, int y), MessageOfBarracks> _localBarracks = new();
        private Dictionary<(int x, int y), MessageOfTrap> _localTraps = new();
        private Dictionary<(int x, int y), MessageOfFarm> _localFarms = new();
        private Dictionary<(int x, int y), MessageOfSpring> _localSprings = new();
        private Dictionary<(int x, int y), MessageOfEconomyResource> _localEconomyResources = new();
        private Dictionary<(int x, int y), MessageOfAdditionResource> _localAdditionResources = new();

        // 基础地图状态缓存
        private PlaceType[,] _baseMapState = new PlaceType[50, 50];
        private bool _mapInitialized = false;

        private readonly MapViewModel _mapViewModel;
        private readonly Logger? _logger;

        public event BuildingEventHandler? OnBuildingEvent;//事件日志回调

        public MapUpdateManager(MapViewModel mapViewModel, Logger? logger = null)
        {
            _mapViewModel = mapViewModel;
            _logger = logger;
        }

        public void ProcessServerUpdate(
            List<MessageOfBarracks> serverBarracks,
            List<MessageOfTrap> serverTraps,
            List<MessageOfFarm> serverFarms,
            List<MessageOfSpring> serverSprings,
            List<MessageOfEconomyResource> serverEconomyResources,
            List<MessageOfAdditionResource> serverAdditionResources,
            MessageOfMap? mapMessage = null)
        {
            try
            {
                // 1. 处理基础地图类型变化
                if (mapMessage != null)
                {
                    ProcessMapTypeChanges(mapMessage);
                }

                // 2. 处理各种动态对象（每个都用try-catch包装，避免一个失败影响其他）
                TryProcessObjects("兵营", () => ProcessBarracksChanges(serverBarracks));
                TryProcessObjects("陷阱", () => ProcessTrapsChanges(serverTraps));
                TryProcessObjects("农场", () => ProcessFarmsChanges(serverFarms));
                TryProcessObjects("泉水", () => ProcessSpringsChanges(serverSprings));
                TryProcessObjects("经济资源", () => ProcessEconomyResourcesChanges(serverEconomyResources));
                TryProcessObjects("加成资源", () => ProcessAdditionResourcesChanges(serverAdditionResources));
            }
            catch (Exception ex)
            {
                _logger?.LogError($"ProcessServerUpdate 发生未捕获的错误: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void TryProcessObjects(string objectType, Action processAction)
        {
            try
            {
                processAction();
            }
            catch (Exception ex)
            {
                _logger?.LogError($"处理{objectType}时发生错误: {ex.Message}");
                // 继续处理其他对象类型
            }
        }

        private void ProcessMapTypeChanges(MessageOfMap mapMessage)
        {
            if (mapMessage?.Rows == null) return;

            for (int i = 0; i < Math.Min(mapMessage.Rows.Count, 50); i++)
            {
                var row = mapMessage.Rows[i];
                if (row?.Cols == null) continue;

                for (int j = 0; j < Math.Min(row.Cols.Count, 50); j++)
                {
                    var newPlaceType = row.Cols[j];

                    if (!_mapInitialized || _baseMapState[i, j] != newPlaceType)
                    {
                        _baseMapState[i, j] = newPlaceType;

                        if (_mapInitialized) // 如果是地图类型变化（非初始化）
                        {
                            _logger?.LogInfo($"地图格子({i},{j})基础类型变化为: {newPlaceType}");
                            // 清除该位置的所有动态对象缓存
                            ClearPositionCache(i, j);
                        }

                        // 更新地图格子基础类型
                        _mapViewModel.UpdateCellTypeFromPlace(i, j, newPlaceType);
                    }
                }
            }

            _mapInitialized = true;
        }

        private void ClearPositionCache(int x, int y)
        {
            var pos = (x, y);

            // 清除各种对象的缓存
            _localBarracks.Remove(pos);
            _localTraps.Remove(pos);
            _localFarms.Remove(pos);
            _localSprings.Remove(pos);
            _localEconomyResources.Remove(pos);
            _localAdditionResources.Remove(pos);
        }

        private void ProcessBarracksChanges(List<MessageOfBarracks> serverBarracks)
        {
            ProcessObjectChanges(
                serverBarracks,
                _localBarracks,
                obj => (obj.X / 1000, obj.Y / 1000),
                UpdateBarracks,
                RemoveBarracks,
                "兵营"
            );
        }

        private void ProcessTrapsChanges(List<MessageOfTrap> serverTraps)
        {
            ProcessObjectChanges(
                serverTraps,
                _localTraps,
                obj => (obj.X / 1000, obj.Y / 1000),
                UpdateTrap,
                RemoveTrap,
                "陷阱"
            );
        }

        private void ProcessFarmsChanges(List<MessageOfFarm> serverFarms)
        {
            ProcessObjectChanges(
                serverFarms,
                _localFarms,
                obj => (obj.X / 1000, obj.Y / 1000),
                UpdateFarm,
                RemoveFarm,
                "农场"
            );
        }

        private void ProcessSpringsChanges(List<MessageOfSpring> serverSprings)
        {
            ProcessObjectChanges(
                serverSprings,
                _localSprings,
                obj => (obj.X / 1000, obj.Y / 1000),
                UpdateSpring,
                RemoveSpring,
                "泉水"
            );
        }

        private void ProcessEconomyResourcesChanges(List<MessageOfEconomyResource> serverResources)
        {
            ProcessObjectChanges(
                serverResources,
                _localEconomyResources,
                obj => (obj.X / 1000, obj.Y / 1000),
                UpdateEconomyResource,
                RemoveEconomyResource,
                "经济资源"
            );
        }

        private void ProcessAdditionResourcesChanges(List<MessageOfAdditionResource> serverResources)
        {
            ProcessObjectChanges(
                serverResources,
                _localAdditionResources,
                obj => (obj.X / 1000, obj.Y / 1000),
                UpdateAdditionResource,
                RemoveAdditionResource,
                "加成资源"
            );
        }

        private void ProcessObjectChanges<T>(
            List<T> serverObjects,
            Dictionary<(int x, int y), T> localObjects,
            Func<T, (int x, int y)> getPosition,
            Action<T> updateAction,
            Action<T> removeAction,
            string objectTypeName)
        {
            // *** 使用坐标作为键的字典 ***
            var serverDict = new Dictionary<(int x, int y), T>();
            var duplicateCount = 0;

            foreach (var obj in serverObjects)
            {
                var pos = getPosition(obj);
                if (serverDict.ContainsKey(pos))
                {
                    duplicateCount++;
                    _logger?.LogWarning($"{objectTypeName} 发现重复坐标: ({pos.x},{pos.y})，跳过重复项");
                    continue; // 跳过重复坐标的对象
                }
                serverDict[pos] = obj;
            }

            if (duplicateCount > 0)
            {
                _logger?.LogWarning($"{objectTypeName} 总共跳过了 {duplicateCount} 个重复坐标的对象");
            }

            // 检查新增和修改
            foreach (var (pos, serverObj) in serverDict)
            {
                if (!localObjects.TryGetValue(pos, out var localObj))
                {
                    // 新增对象
                    updateAction(serverObj);
                    localObjects[pos] = serverObj;

                    //生成建造事件日志
                    var eventMessage = GenerateBuildingEventMessage(serverObj, objectTypeName, "建造", pos);
                    if (!string.IsNullOrEmpty(eventMessage))
                    {
                        OnBuildingEvent?.Invoke(eventMessage, "INFO");
                    }

                    _logger?.LogInfo($"新增{objectTypeName}: Pos=({pos.x},{pos.y})");
                }
                else if (!ObjectsEqual(localObj, serverObj))
                {
                    // 修改对象 - 添加详细的变化日志
                    _logger?.LogDebug($"更新{objectTypeName}: Pos=({pos.x},{pos.y}), 变化详情: {GetChangeDetails(localObj, serverObj, objectTypeName)}");

                    updateAction(serverObj);
                    localObjects[pos] = serverObj;
                }
            }

            // 检查删除
            var removedPositions = localObjects.Keys.Except(serverDict.Keys).ToList();
            foreach (var pos in removedPositions)
            {
                var removedObj = localObjects[pos];
                removeAction(removedObj);
                localObjects.Remove(pos);

                // *** 生成摧毁事件日志 ***
                var eventMessage = GenerateBuildingEventMessage(removedObj, objectTypeName, "摧毁", pos);
                if (!string.IsNullOrEmpty(eventMessage))
                {
                    OnBuildingEvent?.Invoke(eventMessage, "WARN"); // 摧毁用警告级别，显示为橙色
                }

                _logger?.LogInfo($"移除{objectTypeName}: Pos=({pos.x},{pos.y})");
            }
        }

        private string GenerateBuildingEventMessage<T>(T obj, string objectTypeName, string action, (int x, int y) pos)
        {
            // 只为重要的建筑类型生成事件日志
            return obj switch
            {
                MessageOfBarracks barracks =>
                    $"{GetTeamName(barracks.TeamId)} 在 ({pos.x},{pos.y}) {action}了兵营",
                MessageOfFarm farm =>
                    $"{GetTeamName(farm.TeamId)} 在 ({pos.x},{pos.y}) {action}了农场",
                MessageOfSpring spring =>
                    $"{GetTeamName(spring.TeamId)} 在 ({pos.x},{pos.y}) {action}了泉水",
                MessageOfTrap trap =>
                    $"{GetTeamName(trap.TeamId)} 在 ({pos.x},{pos.y}) {action}了{GetTrapTypeName(trap.TrapType)}",
                // 经济资源和加成资源通常不需要建造/摧毁日志，因为是地图固有的
                // 如果需要的话，可以取消下面的注释：//只有被摧毁的资源才生成摧毁日志
                MessageOfEconomyResource er when action == "摧毁" =>
                    $"({pos.x},{pos.y}) 的经济资源被采集完毕",
                MessageOfAdditionResource ar when action == "摧毁" =>
                    $"({pos.x},{pos.y}) 的{GetAdditionResourceTypeName(ar.AdditionResourceType)}被击败", 
                _ => "" // 其他类型不生成事件日志
            };
        }


        // *** 获取变化详情的方法 ***
        private string GetChangeDetails<T>(T oldObj, T newObj, string objectTypeName)
        {
            return (oldObj, newObj) switch
            {
                (MessageOfBarracks old, MessageOfBarracks @new) =>
                    $"HP: {old.Hp} -> {@new.Hp}, TeamId: {old.TeamId} -> {@new.TeamId}",
                (MessageOfFarm old, MessageOfFarm @new) =>
                    $"HP: {old.Hp} -> {@new.Hp}, TeamId: {old.TeamId} -> {@new.TeamId}",
                (MessageOfSpring old, MessageOfSpring @new) =>
                    $"HP: {old.Hp} -> {@new.Hp}, TeamId: {old.TeamId} -> {@new.TeamId}",
                (MessageOfEconomyResource old, MessageOfEconomyResource @new) =>
                    $"Process: {old.Process} -> {@new.Process}, Type: {old.EconomyResourceType} -> {@new.EconomyResourceType}, State: {old.EconomyResourceState} -> {@new.EconomyResourceState}",
                (MessageOfAdditionResource old, MessageOfAdditionResource @new) =>
                    $"HP: {old.Hp} -> {@new.Hp}, Type: {old.AdditionResourceType} -> {@new.AdditionResourceType}, State: {old.AdditionResourceState} -> {@new.AdditionResourceState}",
                (MessageOfTrap old, MessageOfTrap @new) =>
                    $"Type: {old.TrapType} -> {@new.TrapType}, TeamId: {old.TeamId} -> {@new.TeamId}",
                _ => "详情未知"
            };
        }

        // 更新方法
        private void UpdateBarracks(MessageOfBarracks barracks)
        {
            _mapViewModel.UpdateBuildingCell(
                barracks.X / 1000,
                barracks.Y / 1000,
                barracks.TeamId == 0 ? "取经队" : "妖怪队",
                "兵营",
                barracks.Hp
            );
        }

        private void UpdateTrap(MessageOfTrap trap)
        {
            _mapViewModel.UpdateTrapCell(
                trap.X / 1000,
                trap.Y / 1000,
                trap.TeamId == 0 ? "取经队" : "妖怪队",
                trap.TrapType == TrapType.Hole ? "陷阱（坑洞）" : "陷阱（牢笼）"
            );
        }

        private void UpdateFarm(MessageOfFarm farm)
        {
            _mapViewModel.UpdateBuildingCell(
                farm.X / 1000,
                farm.Y / 1000,
                farm.TeamId == 0 ? "取经队" : "妖怪队",
                "农场",
                farm.Hp
            );
        }

        private void UpdateSpring(MessageOfSpring spring)
        {
            _mapViewModel.UpdateBuildingCell(
                spring.X / 1000,
                spring.Y / 1000,
                spring.TeamId == 0 ? "取经队" : "妖怪队",
                "泉水",
                spring.Hp
            );
        }

        private void UpdateEconomyResource(MessageOfEconomyResource resource)
        {
            string resourceType = "经济资源";

            _mapViewModel.UpdateResourceCell(
                resource.X / 1000,
                resource.Y / 1000,
                resourceType,
                resource.Process
            );
        }

        private void UpdateAdditionResource(MessageOfAdditionResource resource)
        {
            //string resourceName = resource.AdditionResourceType switch
            //{
            //    AdditionResourceType.LifePool1 => "生命之泉(1)",
            //    AdditionResourceType.LifePool2 => "生命之泉(2)",
            //    AdditionResourceType.LifePool3 => "生命之泉(3)",
            //    AdditionResourceType.CrazyMan1 => "狂战士之力(1)",
            //    AdditionResourceType.CrazyMan2 => "狂战士之力(2)",
            //    AdditionResourceType.CrazyMan3 => "狂战士之力(3)",
            //    AdditionResourceType.QuickStep => "疾步之灵",
            //    AdditionResourceType.WideView => "视野之灵",
            //    _ => "加成资源(未知)"
            //};

            string resourceName = GetAdditionResourceTypeName(resource.AdditionResourceType);

            _mapViewModel.UpdateAdditionResourceCell(
                resource.X / 1000,
                resource.Y / 1000,
                resourceName,
                resource.Hp
            );
        }

        // 移除方法 - 重置为基础地图类型
        private void RemoveBarracks(MessageOfBarracks barracks)
        {
            _mapViewModel.ResetCellToBaseType(barracks.X / 1000, barracks.Y / 1000, _baseMapState);
        }

        private void RemoveTrap(MessageOfTrap trap)
        {
            _mapViewModel.ResetCellToBaseType(trap.X / 1000, trap.Y / 1000, _baseMapState);
        }

        private void RemoveFarm(MessageOfFarm farm)
        {
            _mapViewModel.ResetCellToBaseType(farm.X / 1000, farm.Y / 1000, _baseMapState);
        }

        private void RemoveSpring(MessageOfSpring spring)
        {
            _mapViewModel.ResetCellToBaseType(spring.X / 1000, spring.Y / 1000, _baseMapState);
        }

        private void RemoveEconomyResource(MessageOfEconomyResource resource)
        {
            _mapViewModel.ResetCellToBaseType(resource.X / 1000, resource.Y / 1000, _baseMapState);
        }

        private void RemoveAdditionResource(MessageOfAdditionResource resource)
        {
            _mapViewModel.ResetCellToBaseType(resource.X / 1000, resource.Y / 1000, _baseMapState);
        }

        // 对象比较方法 - 改进版本
        private bool ObjectsEqual<T>(T obj1, T obj2)
        {
            // 针对不同类型实现具体的比较逻辑
            return obj1 switch
            {
                MessageOfBarracks b1 when obj2 is MessageOfBarracks b2 =>
                    b1.Hp == b2.Hp && b1.TeamId == b2.TeamId && b1.X == b2.X && b1.Y == b2.Y,
                MessageOfTrap t1 when obj2 is MessageOfTrap t2 =>
                    t1.TrapType == t2.TrapType && t1.TeamId == t2.TeamId && t1.X == t2.X && t1.Y == t2.Y,
                MessageOfFarm f1 when obj2 is MessageOfFarm f2 =>
                    f1.Hp == f2.Hp && f1.TeamId == f2.TeamId && f1.X == f2.X && f1.Y == f2.Y,
                MessageOfSpring s1 when obj2 is MessageOfSpring s2 =>
                    s1.Hp == s2.Hp && s1.TeamId == s2.TeamId && s1.X == s2.X && s1.Y == s2.Y,
                MessageOfEconomyResource er1 when obj2 is MessageOfEconomyResource er2 =>
                    er1.Process == er2.Process && er1.EconomyResourceType == er2.EconomyResourceType &&
                    er1.EconomyResourceState == er2.EconomyResourceState && er1.X == er2.X && er1.Y == er2.Y,
                MessageOfAdditionResource ar1 when obj2 is MessageOfAdditionResource ar2 =>
                    ar1.Hp == ar2.Hp && ar1.AdditionResourceType == ar2.AdditionResourceType &&
                    ar1.AdditionResourceState == ar2.AdditionResourceState && ar1.X == ar2.X && ar1.Y == ar2.Y,
                _ => EqualityComparer<T>.Default.Equals(obj1, obj2)
            };
        }


        // *** 新增：获取队伍名称的辅助方法 ***
        private string GetTeamName(long teamId)
        {
            return teamId switch
            {
                0 => "取经队",
                1 => "妖怪队",
                _ => "未知队伍"
            };
        }

        // *** 新增：获取陷阱类型名称的辅助方法 ***
        private string GetTrapTypeName(TrapType trapType)
        {
            return trapType switch
            {
                TrapType.Hole => "坑洞陷阱",
                TrapType.Cage => "牢笼陷阱",
                _ => "陷阱"
            };
        }

        // *** 新增：获取加成资源类型名称的辅助方法（如果需要的话） ***
        private string GetAdditionResourceTypeName(AdditionResourceType type)
        {
            return type switch
            {
                AdditionResourceType.LifePool1 => "生命之泉(1)",
                AdditionResourceType.LifePool2 => "生命之泉(2)",
                AdditionResourceType.LifePool3 => "生命之泉(3)",
                AdditionResourceType.CrazyMan1 => "狂战士之力(1)",
                AdditionResourceType.CrazyMan2 => "狂战士之力(2)",
                AdditionResourceType.CrazyMan3 => "狂战士之力(3)",
                AdditionResourceType.QuickStep => "疾步之灵",
                AdditionResourceType.WideView => "视野之灵",
                _ => "加成资源"
            };
        }

    }
}