# 临时CMakeLists，仅供本地调试用
cmake_minimum_required(VERSION 3.5)

project(THUAI8_CAPI VERSION 1.0)

set(CMAKE_CXX_STANDARD 17)

set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -O2 -pthread")

if(NOT MSVC)
    set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -Wall -Wextra -Wpedantic")
endif()

aux_source_directory(./API/src CPP_LIST)
aux_source_directory(./proto PROTO_CPP_LIST)

set(Protobuf_DIR "/usr/local/lib/cmake/protobuf")
find_package(Protobuf CONFIG REQUIRED)
find_package(gRPC CONFIG REQUIRED)
find_package(absl CONFIG REQUIRED)

message(STATUS "Using protobuf ${Protobuf_VERSION}")
message(STATUS "Using gRPC ${gRPC_VERSION}")
message(STATUS "Using absl ${absl_VERSION}")

message(STATUS "absl found at ${absl_DIR}")

add_executable(capi ${CPP_LIST} ${PROTO_CPP_LIST})

target_include_directories(capi PUBLIC ${PROJECT_SOURCE_DIR}/proto ${PROJECT_SOURCE_DIR}/API/include ${PROJECT_SOURCE_DIR}/tclap/include ${PROJECT_SOURCE_DIR}/spdlog/include ${PROJECT_SOURCE_DIR}/fmt)

target_link_libraries(capi
    protobuf::libprotobuf
    gRPC::grpc
    gRPC::grpc++_reflection
    gRPC::grpc++
    absl::synchronization
    absl::base
)
