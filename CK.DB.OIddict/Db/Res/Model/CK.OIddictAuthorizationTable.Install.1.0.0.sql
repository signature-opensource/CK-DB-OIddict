create table CK.tOIddictAuthorization
(
    AuthorizationId uniqueidentifier                               not null,
    ApplicationId   uniqueidentifier                               not null,
    CreationDate    datetime2(2)                                   not null,
    Properties      nvarchar(max) collate LATIN1_GENERAL_100_BIN2, -- determine size
    Scopes          nvarchar(max) collate LATIN1_GENERAL_100_BIN2, -- determine size
    Status          nvarchar(8) collate LATIN1_GENERAL_100_BIN2    not null,
    Subject         int not null,
    Type            nvarchar(9) collate LATIN1_GENERAL_100_BIN2    not null,

    constraint PK_OIddictAuthorization primary key nonclustered (AuthorizationId),
    constraint FK_OIddictAuthorization_OIddictApplication_ApplicationId
        foreign key (ApplicationId) references CK.tOIddictApplication
);

create nonclustered index IX_OIddictAuthorization_ApplicationId_Status_Subject_Type
    on CK.tOIddictAuthorization (ApplicationId, Status, Subject, Type);
