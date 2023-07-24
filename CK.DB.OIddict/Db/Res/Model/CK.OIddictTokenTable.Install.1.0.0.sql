create table CK.tOIddictToken
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

    constraint PK_OIddictToken primary key nonclustered (TokenId),
    constraint FK_OIddictToken_OIddictApplication_ApplicationId
        foreign key (ApplicationId) references CK.tOIddictApplication,
    constraint FK_OIddictToken_OIddictAuthorization_AuthorizationId
        foreign key (AuthorizationId) references CK.tOIddictAuthorization
);

create index IX_OpIddictToken_ApplicationId_Status_Subject_Type
    on CK.tOIddictToken (ApplicationId, Status, Subject, Type);

create index IX_OIddictToken_AuthorizationId
    on CK.tOIddictToken (AuthorizationId);

create unique index IX_OIddictToken_ReferenceId
    on CK.tOIddictToken (ReferenceId)
    where ReferenceId is not null;

