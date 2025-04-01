# THUAI8 开发者指南

### 访问开发者模式

1. 打开"帮助"页面
2. 在3秒内快速连续点击页面顶部的"THUAI8 帮助"标题5次

### 配置密钥步骤

1. 在开发者模式界面输入腾讯云SecretID和SecretKey
2. 设置加密密码（仅用于本地加密，不会被保存）
3. 点击"生成加密密钥"按钮
4. 将生成的secured_key.csv文件添加为嵌入式资源
5. 重新编译项目

### 嵌入式资源配置

项目已配置自动识别Resources\Raw目录下的secured_key.csv文件作为嵌入式资源：

```xml
<ItemGroup>
    <EmbeddedResource Include="Resources\Raw\secured_key.csv" Condition="Exists('Resources\Raw\secured_key.csv')" />
</ItemGroup>
```

**添加方法**：
1. **VS界面**：创建Resources/Raw文件夹 → 添加文件 → 属性设为"嵌入式资源"
2. **手动方式**：复制文件到Resources\Raw目录 → 确认项目文件包含上述XML配置


---

*注意：请妥善保管加密密码。如果忘记密码，将需要重新生成密钥。* 