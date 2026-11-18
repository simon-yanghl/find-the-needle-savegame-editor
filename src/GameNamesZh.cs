namespace FtnEditor;

/// <summary>根据上游英文标签及内部 ID 暂译，未核实为游戏官方译名。显示 ID 便于核对。</summary>
public static class GameNamesZh
{
    public static string Name(string id) => Names.TryGetValue(id ?? "", out var name) ? name : Ui.Pretty(id);
    public static readonly Dictionary<string, string> Names = new()
    {
        ["yard"] = "场地", ["hand_carry"] = "徒手搬运", ["sand_shovel"] = "沙铲", ["toy_shovel_size"] = "沙铲容量",
        ["spade"] = "铁锹", ["shovel_size"] = "铁锹容量", ["pitchfork"] = "干草叉", ["fork_size"] = "干草叉容量",
        ["broom"] = "扫帚", ["broom_bristles"] = "扫帚刷毛", ["bucket"] = "桶", ["bucket_size"] = "桶容量",
        ["wheelbarrow"] = "独轮手推车", ["barrow_size"] = "手推车容量", ["dump_hatch"] = "卸料舱门", ["smooth_pour"] = "平稳倾倒",
        ["yard_vac"] = "场地吸草机", ["vac_bin"] = "吸草机储箱", ["vac_suction"] = "吸草机吸力", ["vac_hose"] = "吸草机软管", ["vac_hose_length"] = "软管长度",
        ["belt"] = "传送带", ["enclosed_belt"] = "封闭式传送带", ["belt_speed"] = "传送带速度", ["belt_links"] = "传送带连接",
        ["belt_pole"] = "传送带支柱", ["belt_gate"] = "传送带闸门", ["belt_meter"] = "传送带计量器", ["underground_belt"] = "地下传送带",
        ["splitter"] = "分流器", ["overflow_gate"] = "溢流闸门", ["t_splitter"] = "T 型分流器", ["compact_splitter"] = "紧凑型分流器",
        ["smart_splitter"] = "智能分流器", ["split_ratio"] = "分流比例", ["splitter_routes"] = "分流路径", ["joiner"] = "合流器",
        ["compact_joiner"] = "紧凑型合流器", ["haystairs"] = "干草阶梯", ["haylift"] = "干草升降机", ["launcher"] = "发射器",
        ["piston_rake"] = "活塞耙", ["rake_bite"] = "活塞耙抓取量", ["rake_speed"] = "活塞耙速度", ["rake_wheels"] = "活塞耙滚轮",
        ["hay_excavator"] = "干草挖掘机", ["arm_small"] = "小型机械臂", ["arm_standard"] = "标准机械臂", ["arm_long"] = "长机械臂",
        ["overflow_arm"] = "溢流机械臂", ["pick_order"] = "抓取顺序", ["arm_speed"] = "机械臂速度", ["arm_payload"] = "机械臂载荷",
        ["drone"] = "无人机", ["drone_collect"] = "无人机收集", ["drone_radius"] = "无人机范围", ["drone_speed"] = "无人机速度",
        ["drone_lift"] = "无人机升力", ["drone_lift_mk3"] = "无人机升力 MK3", ["skyline_crane"] = "架空起重机", ["switch_box"] = "开关箱", ["light_sensor"] = "光照传感器",
        ["compressor"] = "打包机", ["compressor_batch"] = "打包批量", ["compressor_speed"] = "打包速度", ["bale_quality"] = "草捆品质", ["tune_compressor"] = "打包机调校",
        ["wrapper"] = "覆膜机", ["wrapper_speed"] = "覆膜速度", ["foil_quality"] = "覆膜品质", ["tune_wrapper"] = "覆膜机调校", ["lighter"] = "打火机",
        ["silo"] = "筒仓", ["silo_rate"] = "筒仓流量", ["silo_capacity"] = "筒仓容量", ["silo_bulk"] = "筒仓批量",
        ["pelletizer"] = "制粒机", ["pellet_batch"] = "制粒批量", ["pellet_speed"] = "制粒速度", ["brick_quality"] = "砖块品质", ["pellet_swivel"] = "制粒机旋转",
        ["panel_press"] = "板材压机", ["panel_quality"] = "板材品质", ["pulper"] = "制浆机", ["pulper_batch"] = "制浆批量", ["pulper_speed"] = "制浆速度", ["pulp_quality"] = "浆料品质", ["tune_pulper"] = "制浆机调校",
        ["paper_machine"] = "造纸机", ["paper_speed"] = "造纸速度", ["paper_quality"] = "纸张品质", ["briquette_press"] = "压块机", ["briquette_speed"] = "压块速度", ["briquette_quality"] = "压块品质",
        ["digester"] = "厌氧消化罐", ["sugar_cooker"] = "煮糖机", ["bioplastic_reactor"] = "生物塑料反应器",
        ["smelter"] = "冶炼炉", ["ingot_quality"] = "金属锭品质", ["fabricator"] = "制造机", ["fabricator_quality"] = "制造品质", ["tune_fabricator"] = "制造机调校",
        ["assembler"] = "组装机", ["assembler_quality"] = "组装品质", ["tune_assembler"] = "组装机调校", ["overclocking"] = "超频",
        ["depot"] = "物资站", ["depot_drone"] = "物资站无人机", ["shelf_limits"] = "货架限额", ["hay_price"] = "干草售价", ["better_prices"] = "更优价格",
        ["electricity"] = "电力", ["power_pole"] = "电线杆", ["generator_output"] = "发电机输出", ["tune_power"] = "电力调校", ["pole_span"] = "电线杆跨距", ["pole_drop"] = "电线杆引线",
        ["underground_power"] = "地下电力", ["gas_plant"] = "燃气厂", ["borehole"] = "钻井", ["pump_output"] = "水泵输出", ["pipe_pump_mk2"] = "管道泵 MK2", ["water_main"] = "供水主管", ["underground_pipe"] = "地下管道",
        ["deck"] = "平台", ["access"] = "通道", ["paint_board"] = "涂装板", ["wall"] = "墙壁", ["roof"] = "屋顶", ["work_lamp"] = "工作灯", ["yard_lights"] = "场地照明", ["yard_fan"] = "场地风扇",
        ["yard_space"] = "场地空间", ["steel_saving"] = "节省钢材", ["build_reach"] = "建造距离", ["stand_move"] = "站立移动", ["jump_pad"] = "弹跳垫", ["blueprint_designer"] = "蓝图设计器", ["blueprint_designer_2"] = "蓝图设计器 II", ["blueprint_designer_3"] = "蓝图设计器 III",
        ["scanner_mk1"] = "扫描仪 MK1", ["scanner_mk2"] = "扫描仪 MK2", ["scanner_mk3"] = "扫描仪 MK3", ["scan_batch"] = "扫描批量", ["scan_speed"] = "扫描速度", ["radar_mk1"] = "雷达 MK1", ["cabinet"] = "针收藏柜", ["drawer_space"] = "抽屉容量",
        ["metal_detector"] = "金属探测器", ["detector_coil"] = "探测线圈", ["detector_pinpoint"] = "精准定位",
        ["work_boots"] = "工作靴", ["strong_back"] = "强健背部", ["arm_load"] = "手臂负重", ["second_wind"] = "恢复体力", ["easy_swing"] = "轻松挥动", ["steady_carry"] = "稳定搬运", ["careful_steps"] = "稳健步伐", ["grab_reach"] = "拾取距离", ["jump_height"] = "跳跃高度", ["soft_landings"] = "轻柔着陆", ["grippy_boots"] = "防滑靴", ["jetpack"] = "喷气背包",
        ["hay_bale"] = "草捆", ["foiled_bale"] = "覆膜草捆", ["straw_panel"] = "秸秆板", ["eco_brick"] = "环保砖", ["hay_pulp"] = "干草浆", ["hay_rod"] = "干草棒", ["feed_disc"] = "饲料片", ["packed_carton"] = "包装纸箱", ["paper_roll"] = "纸卷",
        ["iron_ingot"] = "铁锭", ["copper_ingot"] = "铜锭", ["wire_spool"] = "线轴", ["gear_set"] = "齿轮组", ["reinforced_panel"] = "加固板", ["motor_core"] = "电机核心", ["motor"] = "电机", ["carbon_fibre"] = "碳纤维", ["white_bobbin"] = "白色线轴", ["carbon_cable"] = "碳纤维缆线", ["carbon_boom"] = "碳纤维悬臂", ["composite_plate"] = "复合板", ["composite_frame"] = "复合框架", ["sugar_sack"] = "袋装糖", ["bioplastic_sheet"] = "生物塑料薄膜",
        ["arm"] = "机械臂", ["stair"] = "楼梯", ["scanner"] = "扫描仪", ["pole"] = "电线杆", ["generator"] = "发电机", ["u_splitter"] = "U 型分流器", ["u_joiner"] = "U 型合流器", ["rake"] = "活塞耙",
        ["flashlight"] = "手电筒", ["lighter_fire"] = "打火机火焰", ["fan_power"] = "风扇功率", ["fan_mouth"] = "风扇风口", ["fan_loads"] = "风扇负载", ["fan_saver"] = "风扇节能", ["tune_pelletizer"] = "制粒机调校", ["lighter_cooldown"] = "打火机冷却", ["digester_output"] = "消化罐产量", ["tune_smelter"] = "冶炼炉调校", ["tune_panel_press"] = "板材压机调校",
        ["beam"] = "横梁", ["catwalk"] = "检修走道", ["ceiling_light"] = "吸顶灯", ["deck_glass"] = "玻璃平台", ["flood_light"] = "泛光灯", ["hatch"] = "舱门", ["ladder"] = "梯子", ["marking"] = "标线", ["needle_socket"] = "针插座", ["paintboard"] = "涂装板", ["pillar"] = "立柱", ["pipe"] = "管道", ["rail"] = "栏杆", ["ramp"] = "斜坡", ["roof_glass"] = "玻璃屋顶", ["roof_glass_pitch"] = "玻璃坡屋顶", ["roof_hatch"] = "屋顶舱门", ["roof_pitch"] = "坡屋顶", ["sign"] = "标牌", ["street_light"] = "路灯", ["wall_belt"] = "传送带墙体", ["wall_door"] = "门框墙体", ["wall_glass"] = "玻璃墙", ["wall_half"] = "半高墙", ["wall_low"] = "矮墙", ["wall_roller"] = "卷帘门墙体", ["wall_slope"] = "斜墙", ["wall_window"] = "窗框墙体", ["water_tee"] = "三通水管", ["worklamp"] = "工作灯",
        ["platform"] = "平台", ["hay_pelletizer"] = "干草制粒机", ["hay_compressor"] = "打包机", ["hay_wrapper"] = "覆膜机", ["hay_generator"] = "发电机", ["hay_silo"] = "筒仓", ["borehole_pump"] = "钻井水泵", ["water_splitter"] = "分水器", ["conveyor_u_splitter"] = "U 型分流器", ["conveyor_t_splitter"] = "T 型分流器", ["conveyor_u_joiner"] = "U 型合流器", ["conveyor_t_joiner"] = "T 型合流器", ["needle_cabinet"] = "针收藏柜", ["conveyor"] = "传送带", ["robotic_arm"] = "机械臂",
        ["debug_tainted"] = "生涯调试标记", ["sizes_backfilled"] = "规模纪录已补齐", ["backfilled"] = "纪录已补齐",
    };
}
