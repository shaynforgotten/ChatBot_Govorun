create database ChatBotResponses;
go

use ChatBotResponses;
go

create table Responses
(
    ID int primary key identity,
    Question nvarchar(MAX),
    Response nvarchar(MAX),
    Timestamp datetime
);
