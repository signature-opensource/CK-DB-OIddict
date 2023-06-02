-- SetupConfig: {}

create procedure CK.sOpenIddictApplicationCreate(
    @ActorId int,
    @ClientId nvarchar(255),
    @ClientSecret nvarchar(255),
    @ConsentType nvarchar(10),
    @DisplayName nvarchar(128),
    @DisplayNames nvarchar(max),
    @Permissions nvarchar(max),
    @PostLogoutRedirectUris nvarchar(max),
    @Properties nvarchar(max),
    @RedirectUris nvarchar(max),
    @Requirements nvarchar(max),
    @Type nvarchar(12),
    @ApplicationIdResult uniqueidentifier output
)
as
begin
    if @ActorId <= 0 throw 50000, 'Security.AnonymousNotAllowed', 1;
    --[beginsp]

    select @ApplicationIdResult = NewId();

    --<PreCreate revert />

    insert into CK.tOpenIddictApplication
    (
        ApplicationId,
        ClientId,
        ClientSecret,
        ConsentType,
        DisplayName,
        DisplayNames,
        Permissions,
        PostLogoutRedirectUris,
        Properties,
        RedirectUris,
        Requirements,
        Type
     )
    values
    (
        @ApplicationIdResult,
        @ClientId,
        @ClientSecret,
        @ConsentType,
        @DisplayName,
        @DisplayNames,
        @Permissions,
        @PostLogoutRedirectUris,
        @Properties,
        @RedirectUris,
        @Requirements,
        @Type
    );

    --<PostCreate />

    --[endsp]
end
