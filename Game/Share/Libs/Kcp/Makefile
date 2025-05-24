# 编译器设置
CC = gcc
AR = ar
CFLAGS = -Wall -O2
LDFLAGS =

# Windows特定设置
ifeq ($(OS),Windows_NT)
    TARGET_STATIC = kcp.lib
    TARGET_SHARED = kcp.dll
    SHARED_FLAGS = -shared
    CFLAGS += -DWIN32
else
    TARGET_STATIC = libkcp.a
    TARGET_SHARED = libkcp.so
    SHARED_FLAGS = -shared -fPIC
    CFLAGS += -fPIC
endif

# 目标文件
OBJS = ikcp.o

# 默认目标
all: static shared

# 静态库
static: $(TARGET_STATIC)

$(TARGET_STATIC): $(OBJS)
	$(AR) rcs $@ $^

# 动态库
shared: $(TARGET_SHARED)

$(TARGET_SHARED): $(OBJS)
	$(CC) $(SHARED_FLAGS) -o $@ $^ $(LDFLAGS)

# 编译源文件
%.o: %.c
	$(CC) $(CFLAGS) -c $< -o $@

# 清理
clean:
	rm -f $(OBJS) $(TARGET_STATIC) $(TARGET_SHARED)

.PHONY: all static shared clean