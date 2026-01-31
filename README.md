"# iottree.net" 

<a href="./README_cn.md" target="_blank">ÖÐÎÄ</a>

iottree.net is the C#.NET Client support library for IOT-Tree Server

The IOT-Tree Server supports gRPC services, thus enabling high-performance client calls across various development languages and technologies.

This project directly encapsulates the gRPC interface using C#, which you can directly call in your .NET program, avoiding the cumbersome definition of the gRPC interface.

Of course, to run this client, you must also deploy your running instance of IOT-Tree Server together.

The IOT-Tree Server itself only supports a management UI and monitoring pages through the Web. In many scenarios, it is necessary for devices to directly open the corresponding working UI upon power-on, therefore, a pure client-side program must be used.

## Benefits of using this client library

1 The IOT-Tree Server has already completed the docking for your on-site devices (such as various PLCs) in the background. 
To implement your own client, you only need to support the tag list that has been organized by the IOT-Tree Server. 
Each tag uses a unique path identifier in a format similar to xxx.xx.xxx.

This way, your client software does not need to consider any device-specific differences, such as data addresses in PLCs. This can greatly simplify your client development process.

2 Using gRPC allows an instance of IOT-Tree Server to support simultaneous access from multiple clients, which can bring significant benefits in some real-world scenarios.

3 If you are running a standalone system, the IOT-Tree Server and your .NET client program should run on the same system to achieve the best performance.

## How to use this library

Please refer to [Implement Your Own .Net Client][iottree_net]

[iottree_net]: https://github.com/bambooww/iot-tree/blob/main/web/doc/en/doc/util/iottree.net.md


