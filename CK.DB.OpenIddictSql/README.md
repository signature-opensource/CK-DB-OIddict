About technical implementation and choices
For developers or this library


The core / main logic is handled by [Stores](OpenIddict/Stores). They are internally called by OpenIddict managers.
Here they provide a way to access sql database created from [Db](Db).
They are also indirectly called from [Cris](Cris) commands and queries.

