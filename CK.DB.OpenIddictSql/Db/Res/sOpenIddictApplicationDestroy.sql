-- SetupConfig: {}

create procedure CK.sOpenIddictApplicationDestroy
(
    @ActorId int,
    @ApplicationId uniqueidentifier
)
as
begin
    if @ActorId <= 0 throw 50000, 'Security.AnonymousNotAllowed', 1;
    --[beginsp]

    --<PreDestroy revert />

    delete from CK.tOpenIddictApplication
    where ApplicationId = @ApplicationId;

    --<PostDestroy />

    --[endsp]
end
