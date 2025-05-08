protoc --version
protoc Message2Clients.proto --cpp_out=.
head -n 30 Message2Clients.pb.h
protoc MessageType.proto --cpp_out=.
protoc Message2Server.proto --cpp_out=.
protoc Services.proto --grpc_out=. --plugin=protoc-gen-grpc=`which grpc_cpp_plugin`
protoc Services.proto --cpp_out=.
chmod -R 755 ./
mv -f ./*.h ../../CAPI/cpp/proto
mv -f ./*.cc ../../CAPI/cpp/proto
