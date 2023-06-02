-- SetupConfig: {}

create procedure CK.sOpenIddictScopeDestroy
(
    @ActorId int,
    @ScopeId uniqueidentifier
)
as
begin
    if @ActorId <= 0 throw 50000, 'Security.AnonymousNotAllowed', 1;
    --[beginsp]

    --<PreDestroy revert />

    delete from CK.tOpenIddictScope
    where ScopeId = @ScopeId;

    --<PostDestroy />

    --[endsp]
end
