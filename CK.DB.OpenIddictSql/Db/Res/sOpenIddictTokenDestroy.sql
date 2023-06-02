-- SetupConfig: {}

create procedure CK.sOpenIddictTokenDestroy
(
    @ActorId int,
    @TokenId uniqueidentifier
)
as
begin
    if @ActorId <= 0 throw 50000, 'Security.AnonymousNotAllowed', 1;
    --[beginsp]

    --<PreDestroy revert />

    delete from CK.tOpenIddictToken
    where TokenId = @TokenId;

    --<PostDestroy />

    --[endsp]
end
