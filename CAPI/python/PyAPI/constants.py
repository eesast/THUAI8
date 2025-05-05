class Constants:
    frameDuration = 50

    numofGridPerCell = 1000
    rows = 50
    cols = 50

    destroyBarracksBonus = 6000
    destroySpringBonus = 3000
    destroyFarmBonus = 4000

    sizeofCharacter = 800
    speed = 2500


class TangSeng:
    maxHp = 1000


class SunWukong:
    maxHp = 200
    commonAttackPower = 30
    attackRange = 1
    cost = 5000


class ZhuBajie:
    maxHp = 300
    commonAttackPower = 20
    attackRange = 2
    cost = 4000


class ShaWujing:
    maxHp = 150
    commonAttackPower = 10
    attackRange = 5
    cost = 3000


class BaiLongma:
    maxHp = 150
    commonAttackPower = 10
    attackRange = 5
    cost = 4000


class JiuLing:
    maxHp = 1000


class HongHaier:
    maxHp = 200
    commonAttackPower = 25
    attackRange = 1
    cost = 5000


class NiuMowang:
    maxHp = 300
    commonAttackPower = 20
    attackRange = 2
    cost = 4000


class TieShan:
    maxHp = 150
    commonAttackPower = 10
    attackRange = 5
    cost = 3000


class ZhiZhujing:
    maxHp = 150
    commonAttackPower = 10
    attackRange = 5
    cost = 3000


class resumption:
    recovery1 = 50
    recovery2 = 100
    recovery3 = 150
    score1 = 2000
    score2 = 3000
    score3 = 4000
    maxHp1 = 200
    maxHp2 = 300
    maxHp3 = 400
    attack = 10


class Attack_Boost:
    attackBoost1 = 10
    attackBoost2 = 15
    attackBoost3 = 20
    time1 = 30
    time2 = 45
    time3 = 60
    score1 = 4000
    score2 = 5000
    score3 = 6000
    maxHp1 = 400
    maxHp2 = 500
    maxHp3 = 600
    attack1 = 10
    attack2 = 15
    attack3 = 20


class Speed_Boost:
    speedBoost1 = 500
    time = 60
    score = 3000
    maxHp = 300
    attack = 10


class View_Boost:
    time = 60
    score = 3000
    maxHp = 300
    attack = 10


class Barracks:
    maxHp = 600
    cost = 10000
    sabotage_score = 6000
    time_cost = 15


class Spring:
    maxHp = 300
    cost = 8000
    sabotage_score = 3000
    time_cost = 10


class Farm:
    maxHp = 400
    cost = 8000
    sabotage_score = 3000
    time_cost = 10


class Hole:
    cost = 1000
    time = 5
    attack = 20
    continous_time = 5


class Cage:
    cost = 1000
    time = 5
    continueous_time = 30


class blood_vial:
    cost1 = 1500
    cost2 = 3000
    cost3 = 4500
    recovery1 = 50
    recovery2 = 100
    recovery3 = 150


class Shield:
    cost1 = 2000
    cost2 = 3500
    cost3 = 5000
    defence1 = 50
    defence2 = 100
    defence3 = 150


class Speed_shoes:
    speedBoost = 500
    time = 60
    cost = 1500


class purification_medicine:
    cost = 2000
    time = 30


class invisibility:
    cost = 4000
    time = 10


class berserk:
    cost = 10000
    time = 30
    attack_boost = 1.2  # 注意这是提升的倍数
    speed_boost = 300  # 注意这是直接叠加
    attack_freq_boost = 1.25  # 注意这是提升的倍数
