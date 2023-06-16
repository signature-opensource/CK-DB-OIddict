-- SetupConfig: {}

create procedure CK.sOpenIddictAuthorizationCreate
(
    @ActorId int,
    @AuthorizationId uniqueidentifier,
    @ApplicationId uniqueidentifier,
    @CreationDate datetime2(2),
    @Properties nvarchar(max),
    @Scopes nvarchar(max),
    @Status nvarchar(8),
    @Subject nvarchar(256),
    @Type nvarchar(9)
)
as
begin
    if @ActorId <= 0 throw 50000, 'Security.AnonymousNotAllowed', 1;
    --[beginsp]

    --<PreCreate revert />

    insert into CK.tOpenIddictAuthorization
    (
        AuthorizationId,
        ApplicationId,
        CreationDate,
        Properties,
        Scopes,
        Status,
        Subject,
        Type
     )
    values
    (
        @AuthorizationId,
        @ApplicationId,
        @CreationDate,
        @Properties,
        @Scopes,
        @Status,
        @Subject,
        @Type
     )

    --<PostCreate />

    --[endsp]
end
