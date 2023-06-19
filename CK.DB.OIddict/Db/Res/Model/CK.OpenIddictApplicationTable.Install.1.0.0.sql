--[beginscript]

create table CK.tOpenIddictApplication
(
    ApplicationId          uniqueidentifier                               not null,
    ClientId               nvarchar(255) collate LATIN1_GENERAL_100_CI_AS not null,
    ClientSecret           nvarchar(255) collate LATIN1_GENERAL_100_BIN2  not null,
    ConsentType            nvarchar(10) collate LATIN1_GENERAL_100_BIN2   not null,
    DisplayName            nvarchar(128) collate LATIN1_GENERAL_100_CI_AS not null,
    DisplayNames           nvarchar(max) collate LATIN1_GENERAL_100_CI_AS,
    Permissions            nvarchar(max) collate LATIN1_GENERAL_100_BIN2, -- determine size
    PostLogoutRedirectUris nvarchar(max) collate LATIN1_GENERAL_100_CI_AS,
    Properties             nvarchar(max) collate LATIN1_GENERAL_100_BIN2, -- determine size
    RedirectUris           nvarchar(max) collate LATIN1_GENERAL_100_CI_AS,
    Requirements           nvarchar(max) collate LATIN1_GENERAL_100_BIN2, -- determine size
    Type                   nvarchar(12) collate LATIN1_GENERAL_100_BIN2,

    constraint PK_OpenIddictApplication primary key nonclustered (ApplicationId)
);

create unique nonclustered index IX_OpenIddictApplication_ClientId
    on CK.tOpenIddictApplication (ClientId);

--[endscript]
