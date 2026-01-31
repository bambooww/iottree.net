"# iottree.net" 
iottree.net是IOT-Tree Server的C#.NET Client支持库
==

IOT-Tree Server支持gRPC服务，因此可以支持各种开发语言和技术的客户端高性能调用。

本项目直接使用C#对gPRC接口进行了封装，你可以直接在你的.net程序中调用，避免了繁琐的gPRC接口定义。

当然，运行此客户端也必须把你的IOT-Tree Server运行实例一起部署。

IOT-Tree Server自身只支持Web方式的管理UI和监控画面，在很多场合需要设备能够直接随着电源的启动打开对应的工作界面，因此必须使用纯客户端方式的程序。

## 使用此客户端库带来的好处

1 IOT-Tree Server在后台已经为你现场设备（如各种PLC）做好了对接，你要实现自己的客户端只需要支持IOT-Tree Server已经整理好的标签列表，每个标签使用类似 xxx.xx.xxx 格式的唯一路径标识。

这样你的客户端软件不需要考虑任何设备各种差异信息——如PLC中的数据地址等。这可以大大简化你的客户端开发过程。

2 使用gPRC可以使得一个IOT-Tree Server实例支持多个客户端同时访问，这在一些现场会有很大的好处。

3 如果你运行的是单机系统，IOT-Tree Server和你的.net客户端程序运行在同一个系统中，这样可以达到最好的性能。

## 如何使用此库

请参考[实现你自己的.Net客户端][iottree_net]

[iottree_net]: https://github.com/bambooww/iot-tree/blob/main/web/doc/cn/doc/util/iottree.net.md

