-- SetupConfig: {}

create procedure CK.sOpenIddictScopeCreate
(
    @ActorId int,
    @Description nvarchar(1024),
    @Descriptions nvarchar (max),
    @DisplayName nvarchar (255),
    @DisplayNames nvarchar (max),
    @ScopeName nvarchar(255),
    @Properties nvarchar (max),
    @Resources nvarchar (max),
    @ScopeIdResult uniqueidentifier output
)
as
begin
    if @ActorId <= 0 throw 50000, 'Security.AnonymousNotAllowed', 1;
    --[beginsp]

    select @ScopeIdResult = NewId();

    --<PreCreate revert />

    insert into CK.tOpenIddictScope
    (
        ScopeId,
        Description,
        Descriptions,
        DisplayName,
        DisplayNames,
        ScopeName,
        Properties,
        Resources
    )
    values
    (
        @ScopeIdResult,
        @Description,
        @Descriptions,
        @DisplayName,
        @DisplayNames,
        @ScopeName,
        @Properties,
        @Resources
    )

    --<PostCreate />

    --[endsp]
end
