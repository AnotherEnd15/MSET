cmake_minimum_required(VERSION 3.10)
project(kcp)

# 设置C标准
set(CMAKE_C_STANDARD 11)

# 添加编译选项
if(MSVC)
    add_compile_options(/W3)
else()
    add_compile_options(-Wall -Wextra)
endif()

# 创建静态库
add_library(kcp STATIC ikcp.c)

# 创建动态库
add_library(kcp_shared SHARED ikcp.c)
set_target_properties(kcp_shared PROPERTIES OUTPUT_NAME kcp)

# 安装规则
install(TARGETS kcp kcp_shared
    ARCHIVE DESTINATION lib
    LIBRARY DESTINATION lib
    RUNTIME DESTINATION bin)
install(FILES ikcp.h DESTINATION include)