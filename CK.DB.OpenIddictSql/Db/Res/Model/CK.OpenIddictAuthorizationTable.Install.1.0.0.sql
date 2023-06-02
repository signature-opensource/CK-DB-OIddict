create table CK.tOpenIddictAuthorization
(
    AuthorizationId uniqueidentifier                               not null,
    ApplicationId   uniqueidentifier                               not null,
    CreationDate    datetime2(2)                                   not null,
--         constraint DF_CK_tOpenIddictAuthorization_CreationDate default (sysutcdatetime()),
    Properties      nvarchar(max) collate LATIN1_GENERAL_100_BIN2, -- determine size
    Scopes          nvarchar(max) collate LATIN1_GENERAL_100_BIN2, -- determine size
    Status          nvarchar(8) collate LATIN1_GENERAL_100_BIN2    not null,
    Subject         nvarchar(256) collate LATIN1_GENERAL_100_CI_AS not null,
    Type            nvarchar(9) collate LATIN1_GENERAL_100_BIN2    not null,

    constraint PK_OpenIddictAuthorization primary key nonclustered (AuthorizationId),
    constraint FK_OpenIddictAuthorization_OpenIddictApplication_ApplicationId
        foreign key (ApplicationId) references CK.tOpenIddictApplication
);

create nonclustered index IX_OpenIddictAuthorization_ApplicationId_Status_Subject_Type
    on CK.tOpenIddictAuthorization (ApplicationId, Status, Subject, Type);
