# RPC with RabbitMQ

## Prerequisites
RabbitMQ installed

## Development tool
Visual studio 2019 with c# .net core 3.1 and RabbitMq.Client 

## Projects
Four projects in the solution include:

| Project | Description |
| ------ | ------ |
| RabbitMq.Config | config the queues and host name |
| RabbitMq | two classes represent the requestor and the service |
| RabbitMq.Requestor | win form represents requestor |
| RabbitMq.Servicer | win form represents servicer |

## RabbitMq.Requestor
Class that initiates the request and the consumer listening to the response from the servicer.
This class is generic so request and response can be any.

## RabbitMq.Servicer
Primarily create consumer to response to the request from requestor, once request handled publish it back to the requestor.
Request handled by generic func delegate. 

## RabbitMq.Requestor & RabbitMq.Servicer
Win form that show how the clases implemented
