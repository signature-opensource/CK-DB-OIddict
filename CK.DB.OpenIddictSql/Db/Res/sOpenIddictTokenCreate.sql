-- SetupConfig: {}

create procedure CK.sOpenIddictTokenCreate
(
    @ActorId int,
    @TokenId uniqueidentifier,
    @ApplicationId uniqueidentifier,
    @AuthorizationId uniqueidentifier,
    @CreationDate datetime2(2),--todo: creationdate
    @ExpirationDate datetime2(2), -- todo: ExpirationDate
    @Payload nvarchar(max),
    @Properties nvarchar (max),
    @RedemptionDate datetime2 (2), -- todo: date
    @ReferenceId uniqueidentifier,
    @Status nvarchar (8),
    @Subject nvarchar (256),
    @Type nvarchar (22)
)
as
begin
    if @ActorId <= 0 throw 50000, 'Security.AnonymousNotAllowed', 1;
    --[beginsp]

    --<PreCreate revert />

    insert into CK.tOpenIddictToken
    (
        TokenId,
        ApplicationId,
        AuthorizationId,
        CreationDate,
        ExpirationDate,
        Payload,
        Properties,
        RedemptionDate,
        ReferenceId,
        Status,
        Subject,
        Type
    )
    values
    (
        @TokenId,
        @ApplicationId,
        @AuthorizationId,
        @CreationDate,
        @ExpirationDate,
        @Payload,
        @Properties,
        @RedemptionDate,
        @ReferenceId,
        @Status,
        @Subject,
        @Type
    )

    --<PostCreate />

    --[endsp]
end
