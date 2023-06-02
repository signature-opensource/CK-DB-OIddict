create table CK.tOpenIddictToken
(
    TokenId         uniqueidentifier                               not null,
    ApplicationId   uniqueidentifier                               not null,
    AuthorizationId uniqueidentifier                               not null,
    CreationDate    datetime2(2)                                   not null,
    ExpirationDate  datetime2(2)                                   not null,
    Payload         nvarchar(max) collate LATIN1_GENERAL_100_BIN2,
    Properties      nvarchar(max) collate LATIN1_GENERAL_100_BIN2, -- determine size
    RedemptionDate  datetime2(2),
    ReferenceId     nvarchar(255) collate LATIN1_GENERAL_100_BIN2,
    Status          nvarchar(8) collate LATIN1_GENERAL_100_BIN2,
    Subject         nvarchar(256) collate LATIN1_GENERAL_100_CI_AS not null,
    Type            nvarchar(22) collate LATIN1_GENERAL_100_BIN2,

    constraint PK_OpenIddictToken primary key nonclustered (TokenId),
    constraint FK_OpenIddictToken_OpenIddictApplication_ApplicationId
        foreign key (ApplicationId) references CK.tOpenIddictApplication,
    constraint FK_OpenIddictToken_OpenIddictAuthorization_AuthorizationId
        foreign key (AuthorizationId) references CK.tOpenIddictAuthorization
);

create index IX_OpenIddictToken_ApplicationId_Status_Subject_Type
    on CK.tOpenIddictToken (ApplicationId, Status, Subject, Type);

create index IX_OpenIddictToken_AuthorizationId
    on CK.tOpenIddictToken (AuthorizationId);

create unique index IX_OpenIddictToken_ReferenceId
    on CK.tOpenIddictToken (ReferenceId)
    where ReferenceId is not null;

