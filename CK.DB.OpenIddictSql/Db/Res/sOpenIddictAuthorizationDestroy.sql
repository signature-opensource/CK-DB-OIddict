-- SetupConfig: {}

create procedure CK.sOpenIddictAuthorizationDestroy
(
    @ActorId int,
    @AuthorizationId uniqueidentifier
)
as
begin
    if @ActorId <= 0 throw 50000, 'Security.AnonymousNotAllowed', 1;
    --[beginsp]

    --<PreDestroy revert />

    delete from CK.tOpenIddictAuthorization
    where AuthorizationId = @AuthorizationId;

    --<PostDestroy />

    --[endsp]
end
