# Discord moderation bot — PlayFab & Photon API reference

Reference for building a Discord bot that pulls player info, cosmetics, live session data, and moderation actions from **PlayFab** and **Photon Realtime**.

- PlayFab REST index: [learn.microsoft.com REST API](https://learn.microsoft.com/en-us/rest/api/playfab/?view=playfab-rest)
- Base URL: `https://{TitleId}.playfabapi.com/{Service}/{Operation}`
- Photon webhooks: [Realtime Webhooks](https://doc.photonengine.com/realtime/current/reference/webhooks)
- Photon Counter API: [Analytics Counter API](https://doc-api.photonengine.com/en/analytics-api/current/intro)

---

## Architecture notes

**PlayFab** holds persistent identity, economy, user data, bans, and (optionally) telemetry.

**Photon Realtime** holds live rooms and connected players. There is **no public REST API** to query “all players in room X” or full per-player live state. You get that through:

1. **Webhooks** (Photon POSTs to your HTTPS backend),
2. A **headless Photon client** (lobby room list / join room and read `Players`),
3. **Your game** writing presence to PlayFab (`WriteTelemetryEvents`) or your own DB,
4. **CloudScript / Azure Functions** (`CloudScript/ExecuteFunction`) returning a normalized mod panel.

---

## Credentials (required access)

| Credential | Used for |
|------------|----------|
| **Title Secret Key** | Most `Admin/*` and `Server/*` (`X-SecretKey`) |
| **Title Entity Token** | Economy v2, CloudScript (Azure Functions), Multiplayer (`X-EntityToken` via `Authentication/GetEntityToken`) |
| **Photon App ID + regions** | Monitor services, Counter API |
| **Photon Counter API token** | Premium / Circle / Enterprise ([dashboard analytics tokens](https://dashboard.photonengine.com/settings/analyticsapitokens)) |
| **Webhook HTTPS base URL** | Photon pushes room/player events to your service |
| **Insights / Data Connections** (optional) | KQL queries at `https://insights.playfab.com/v1/rest/query` |

---

## Minimum viable endpoint set

Typical Discord mod bot (PlayFab backend):

1. `Authentication/GetEntityToken`
2. `Server/GetPlayFabIDsFromServerCustomIDs` (or your ID mapping)
3. `Server/GetPlayerCombinedInfo` **or** `GetUserInternalData` + `Inventory/GetInventoryItems`
4. `Catalog/SearchItems` or `Catalog/GetItems` (metadata for cosmetic IDs)
5. `Admin/BanUsers`, `Admin/GetUserBans`, `Admin/RevokeBans`
6. `Server/ReportPlayer` (if surfacing in-game reports)
7. `CloudScript/ExecuteFunction` — custom “mod panel” JSON from the title

Add for live multiplayer (Photon):

8. Webhooks: `PathBeforeJoin`, `PathJoin`, `PathLeave`, `PathClose` (minimum)
9. Counter API: `ccu`, `rooms`
10. Optional: headless Photon client for `/rooms` listing
11. Optional: game → `Event/WriteTelemetryEvents` or external store for `/online`

---

## Cosmetics storage (map your title)

| Storage | API |
|---------|-----|
| Legacy catalog items | `Server/GetUserInventory` + `Admin/GetCatalogItems` |
| Economy v2 items | `Inventory/GetInventoryItems` + `Catalog/GetItem` |
| Equipped loadout JSON | `GetUserData` / `GetUserInternalData` / `Object/GetObjects` |
| In-match only (Photon) | Room/player custom properties via webhooks or in-room client |

---

# PlayFab REST operations

## Authentication — `/Authentication/`

| Operation | Purpose |
|-----------|---------|
| `GetEntityToken` | Secret key or ticket → entity token |
| `ValidateEntityToken` | Validate client/service tokens |
| `AuthenticateGameServerWithCustomId` | Game server entity for monitors |
| `Delete` | Remove `game_server` entity |

---

## Admin — Account Management — `/Admin/`

| Operation | Purpose |
|-----------|---------|
| `BanUsers` | Ban by PlayFab ID (+ optional IP) |
| `GetUserBans` | Ban history |
| `RevokeBans` | Revoke by ban ID |
| `RevokeAllBansForUser` | Clear all bans for user |
| `UpdateBans` | Edit ban metadata |
| `GetUserAccountInfo` | Lookup (PII — restrict bot permissions) |
| `GetPlayerProfile` | Profile, last login, display name, etc. |
| `GetPlayerIdFromAuthToken` | Token → PlayFab ID |
| `GetPlayedTitleList` | Titles played (master account) |
| `UpdateUserTitleDisplayName` | Force rename |
| `ExportMasterPlayerData` | GDPR / export bundle |
| `DeletePlayer` | Remove title player |
| `DeleteMasterPlayerAccount` | Delete master account |
| `DeleteMasterPlayerEventData` | Wipe PlayStream/telemetry for user |
| `DeleteMembershipSubscription` | Subscription admin |
| `SetMembershipOverride` | Subscription override |
| `ResetPassword` | Account recovery |
| `SendAccountRecoveryEmail` | Account recovery |
| `DeleteTitle` | Title teardown |

---

## Admin — Player Data Management — `/Admin/`

| Operation | Purpose |
|-----------|---------|
| `GetUserData` | Client-readable KV |
| `GetUserReadOnlyData` | Read-only KV |
| `GetUserInternalData` | Server-only KV (common for cosmetics) |
| `GetUserPublisherData` | Publisher KV |
| `GetUserPublisherReadOnlyData` | Publisher read-only KV |
| `GetUserPublisherInternalData` | Publisher internal KV |
| `ListPlayerCustomProperties` | List custom properties |
| `GetPlayerCustomProperty` | Single property |
| `UpdateUserData` | Write client KV |
| `UpdateUserReadOnlyData` | Write read-only KV |
| `UpdateUserInternalData` | Write internal KV |
| `UpdateUserPublisherData` | Write publisher KV |
| `UpdateUserPublisherReadOnlyData` | Write publisher read-only KV |
| `UpdateUserPublisherInternalData` | Write publisher internal KV |
| `UpdatePlayerCustomProperties` | Update properties |
| `DeletePlayerCustomProperties` | Delete properties |
| `GetPlayerStatisticDefinitions` | Stat definitions |
| `GetPlayerStatisticVersions` | Stat versions |
| `CreatePlayerStatisticDefinition` | Create stat |
| `UpdatePlayerStatisticDefinition` | Update stat |
| `IncrementPlayerStatisticVersion` | Reset stat season |
| `ResetUserStatistics` | Wipe player stats |
| `GetDataReport` | Report download URL |
| `RefundPurchase` | Legacy economy refund |
| `ResolvePurchaseDispute` | Legacy dispute |

---

## Admin — Player Item Management (legacy economy) — `/Admin/`

| Operation | Purpose |
|-----------|---------|
| `GetUserInventory` | Owned items |
| `GrantItemsToUsers` | Grant items |
| `RevokeInventoryItem` | Revoke one item |
| `RevokeInventoryItems` | Revoke batch |
| `AddUserVirtualCurrency` | Add currency |
| `SubtractUserVirtualCurrency` | Remove currency |
| `CheckLimitedEditionItemAvailability` | Limited edition check |
| `IncrementLimitedEditionItemAvailability` | Limited edition increment |

---

## Admin — Play Stream — `/Admin/`

| Operation | Purpose |
|-----------|---------|
| `GetPlayerTags` | List tags |
| `AddPlayerTag` | Add mod label |
| `RemovePlayerTag` | Remove tag |
| `GetPlayerSegments` | Segment membership |
| `GetAllSegments` | All segment definitions |
| `GetSegmentPlayerCount` | Count in segment |
| `ExportPlayersInSegment` | Start segment export |
| `GetSegmentExport` | Download export result |

Same eight operations exist under **Server/PlayStream** (use Admin or Server consistently, not both).

---

## Admin — Title-Wide Data Management — `/Admin/`

| Operation | Purpose |
|-----------|---------|
| `GetCatalogItems` | Legacy catalog read |
| `SetCatalogItems` | Legacy catalog write |
| `UpdateCatalogItems` | Legacy catalog patch |
| `GetStoreItems` | Legacy store |
| `SetStoreItems` | Legacy store write |
| `UpdateStoreItems` | Legacy store patch |
| `DeleteStore` | Delete store |
| `ListVirtualCurrencyTypes` | Legacy VC list |
| `AddVirtualCurrencyTypes` | Add VC |
| `RemoveVirtualCurrencyTypes` | Remove VC |
| `GetRandomResultTables` | Loot tables |
| `UpdateRandomResultTables` | Update loot tables |
| `GetTitleData` | Title KV (client-readable) |
| `GetTitleInternalData` | Title KV (server-only) |
| `SetTitleData` | Set title KV |
| `SetTitleInternalData` | Set internal title KV |
| `SetTitleDataAndOverrides` | Overrides |
| `DeleteTitleDataOverride` | Delete override |
| `GetPublisherData` | Publisher settings |
| `AddNews` | News feed |
| `AddLocalizedNews` | Localized news |
| `SetupPushNotification` | Push ARN setup |

---

## Server — Account Management — `/Server/`

### Moderation & lookup

| Operation | Purpose |
|-----------|---------|
| `GetUserAccountInfo` | User lookup |
| `GetPlayerProfile` | Profile |
| `BanUsers` | Ban |
| `GetUserBans` | List bans |
| `RevokeBans` | Revoke by ID |
| `RevokeAllBansForUser` | Revoke all for user |
| `UpdateBans` | Update bans |
| `DeletePlayer` | Delete title player |

### ID resolution (Discord / Roblox / platforms)

| Operation | Purpose |
|-----------|---------|
| `GetPlayFabIDsFromServerCustomIDs` | Custom ID → PlayFab |
| `GetServerCustomIDsFromPlayFabIDs` | PlayFab → custom ID |
| `GetPlayFabIDsFromGenericIDs` | Generic service ID |
| `AddGenericID` / `RemoveGenericID` | Generic ID links |
| `LinkServerCustomId` / `UnlinkServerCustomId` | Custom ID link |
| `GetPlayFabIDsFromSteamIDs` | Steam |
| `GetPlayFabIDsFromSteamNames` | Steam persona |
| `GetPlayFabIDsFromXboxLiveIDs` | Xbox |
| `GetPlayFabIDsFromPSNAccountIDs` | PlayStation |
| `GetPlayFabIDsFromPSNOnlineIDs` | PlayStation online ID |
| `GetPlayFabIDsFromNintendoServiceAccountIds` | Nintendo |
| `GetPlayFabIDsFromNintendoSwitchDeviceIds` | Switch device |
| `GetPlayFabIDsFromTwitchIDs` | Twitch |
| `GetPlayFabIDsFromFacebookIDs` | Facebook |
| `GetPlayFabIDsFromFacebookInstantGamesIds` | FB instant games |
| `GetPlayFabIDsFromBattleNetAccountIds` | Battle.net |
| `GetPlayFabIDsFromOpenIdSubjectIdentifiers` | OpenID |

### Other Server Account ops (platform link/unlink, push, email)

Includes `Link*` / `Unlink*` for Steam, Xbox, PSN, Nintendo, Twitch, etc.; `UpdateAvatarUrl`; push notification and email template helpers. Full list: [Server Account Management](https://learn.microsoft.com/en-us/rest/api/playfab/server/account-management?view=playfab-rest).

---

## Server — Player Data Management — `/Server/`

| Operation | Purpose |
|-----------|---------|
| **`GetPlayerCombinedInfo`** | Multi-flag profile/inventory/data read |
| `GetUserData` | Client KV |
| `GetUserReadOnlyData` | Read-only KV |
| `GetUserInternalData` | Internal KV |
| `GetUserPublisherData` | Publisher KV |
| `GetUserPublisherReadOnlyData` | Publisher read-only |
| `GetUserPublisherInternalData` | Publisher internal |
| `ListPlayerCustomProperties` | Properties list |
| `GetPlayerCustomProperty` | Single property |
| `UpdatePlayerCustomProperties` | Update properties |
| `DeletePlayerCustomProperties` | Delete properties |
| `UpdateUserData` | Write client KV |
| `UpdateUserReadOnlyData` | Write read-only KV |
| `UpdateUserInternalData` | Write internal KV |
| `UpdateUserPublisherData` | Write publisher KV |
| `UpdateUserPublisherReadOnlyData` | Write publisher read-only |
| `UpdateUserPublisherInternalData` | Write publisher internal |
| `GetPlayerStatistics` | Statistics |
| `UpdatePlayerStatistics` | Update stats |
| `GetPlayerStatisticVersions` | Stat versions |
| `GetLeaderboard` | Leaderboard |
| `GetLeaderboardAroundUser` | Leaderboard around player |
| `GetFriendLeaderboard` | Friend leaderboard |

---

## Server — Player Item Management (legacy economy) — `/Server/`

| Operation | Purpose |
|-----------|---------|
| `GetUserInventory` | Inventory |
| `GetCharacterInventory` | Character inventory |
| `GrantItemsToUser` | Grant to user |
| `GrantItemsToUsers` | Batch grant |
| `GrantItemsToCharacter` | Grant to character |
| `RevokeInventoryItem` | Revoke one |
| `RevokeInventoryItems` | Revoke batch |
| `UpdateUserInventoryItemCustomData` | Per-instance metadata |
| `ConsumeItem` | Consume uses |
| `ModifyItemUses` | Modify uses |
| `UnlockContainerInstance` | Open container instance |
| `UnlockContainerItem` | Open container by item |
| `AddUserVirtualCurrency` | Add VC |
| `SubtractUserVirtualCurrency` | Subtract VC |
| `AddCharacterVirtualCurrency` | Character VC add |
| `SubtractCharacterVirtualCurrency` | Character VC subtract |
| `MoveItemToCharacterFromUser` | Move item |
| `MoveItemToUserFromCharacter` | Move item |
| `MoveItemToCharacterFromCharacter` | Move item |
| `RedeemCoupon` | Coupon |
| `EvaluateRandomResultTable` | Random table |
| `GetRandomResultTables` | Table config |
| **`ReportPlayer`** | Player report pipeline |

---

## Server — Characters — `/Server/`

| Operation | Purpose |
|-----------|---------|
| `GetAllUsersCharacters` | List characters |
| `GetCharacterStatistics` | Character stats |
| `UpdateCharacterStatistics` | Update character stats |
| `GetCharacterLeaderboard` | Character leaderboard |
| `GetLeaderboardAroundCharacter` | Around character |
| `GetLeaderboardForUserCharacters` | User’s characters on board |
| `GrantCharacterToUser` | Grant character |
| `DeleteCharacterFromUser` | Delete character |

---

## Server — Server-Side Cloud Script (legacy JS) — `/Server/`

| Operation | Purpose |
|-----------|---------|
| `ExecuteCloudScript` | Run legacy CloudScript with `PlayFabId` |

---

## CloudScript (Azure Functions) — `/CloudScript/`

| Operation | Purpose |
|-----------|---------|
| **`ExecuteFunction`** | Run registered Azure Function (recommended mod panel) |
| `ExecuteEntityCloudScript` | Entity-scoped legacy script |
| `GetFunction` | Get function registration |
| `ListFunctions` | List all functions |
| `ListHttpFunctions` | HTTP-triggered functions |
| `ListQueuedFunctions` | Queue-triggered |
| `ListEventHubFunctions` | Event Hub–triggered |
| `RegisterHttpFunction` | Register HTTP function |
| `RegisterQueuedFunction` | Register queue function |
| `RegisterEventHubFunction` | Register Event Hub function |
| `UnregisterFunction` | Unregister |
| `PostFunctionResultForEntityTriggeredAction` | PlayStream event |
| `PostFunctionResultForFunctionExecution` | PlayStream event |
| `PostFunctionResultForPlayerTriggeredAction` | PlayStream event |
| `PostFunctionResultForScheduledTask` | PlayStream event |

---

## Economy v2 — Catalog — `/Catalog/` (entity token)

| Operation | Purpose |
|-----------|---------|
| `SearchItems` | Fuzzy/OData catalog search |
| `GetItem` | Single item (fresh) |
| `GetItems` | Batch items |
| `GetItemContainers` | Bundles/stores containing item |
| `GetCatalogConfig` | Catalog config |
| `UpdateCatalogConfig` | Update config |
| `GetDraftItem` | Working copy |
| `GetDraftItems` | Draft list |
| `GetEntityDraftItems` | Entity drafts |
| `CreateDraftItem` | Create draft |
| `UpdateDraftItem` | Update draft |
| `PublishDraftItem` | Publish |
| `GetItemPublishStatus` | Publish status |
| `DeleteItem` | Remove item |
| `CreateUploadUrls` | Asset upload URLs |
| `GetItemModerationState` | UGC moderation state |
| `SetItemModerationState` | Set moderation state |
| `ReportItem` | Report item |
| `GetItemReviews` | Reviews |
| `GetItemReviewSummary` | Review summary |
| `GetEntityItemReview` | Entity’s review |
| `ReviewItem` | Submit review |
| `ReportItemReview` | Report review |
| `SubmitItemReviewVote` | Vote on review |
| `TakedownItemReviews` | Takedown reviews |
| `DeleteEntityItemReviews` | Delete entity reviews |

---

## Economy v2 — Inventory — `/Inventory/` (entity token)

| Operation | Purpose |
|-----------|---------|
| **`GetInventoryItems`** | Player inventory (v2) |
| `GetInventoryCollectionIds` | Collection IDs |
| `GetTransactionHistory` | Purchase/grant history |
| `AddInventoryItems` | Grant |
| `SubtractInventoryItems` | Remove quantity |
| `DeleteInventoryItems` | Delete stacks |
| `UpdateInventoryItems` | Update stacks |
| `ExecuteInventoryOperations` | Batch inventory ops |
| `ExecuteTransferOperations` | Batch transfers |
| `TransferInventoryItems` | Transfer |
| `PurchaseInventoryItems` | Purchase |
| `GetInventoryOperationStatus` | Async status |
| `DeleteInventoryCollection` | Delete collection |
| `RedeemSteamInventoryItems` | Steam IAP |
| `RedeemGooglePlayInventoryItems` | Google IAP |
| `RedeemAppleAppStoreInventoryItems` | Apple IAP |
| `RedeemAppleAppStoreWithJwsInventoryItems` | Apple JWS |
| `RedeemMicrosoftStoreInventoryItems` | Microsoft IAP |
| `RedeemNintendoEShopInventoryItems` | Nintendo IAP |
| `RedeemPlayStationStoreInventoryItems` | PlayStation IAP |

---

## Profiles — `/Profile/` (entity token)

| Operation | Purpose |
|-----------|---------|
| `GetProfile` | Entity profile |
| `GetProfiles` | Batch profiles |
| `GetTitlePlayersFromMasterPlayerAccountIds` | Master → title player |
| `GetTitlePlayersFromXboxLiveIDs` | Xbox → title player |
| `SetDisplayName` | Set display name |
| `SetProfileLanguage` | Language |
| `SetProfilePolicy` | Profile policy |
| `GetGlobalPolicy` | Global policy |
| `SetGlobalPolicy` | Set global policy |

---

## Data — `/Object/`, `/File/` (entity token)

| Operation | Purpose |
|-----------|---------|
| `GetObjects` | Entity JSON objects |
| `SetObjects` | Set objects |
| `GetFiles` | File metadata |
| `InitiateFileUploads` | Start upload |
| `FinalizeFileUploads` | Complete upload |
| `AbortFileUploads` | Abort upload |
| `DeleteFiles` | Delete files |

---

## Events — `/Event/`

| Operation | Purpose |
|-----------|---------|
| `WriteEvents` | Custom PlayStream events (`custom.*`) |
| `WriteTelemetryEvents` | Telemetry (bypass PlayStream) |
| `CreateTelemetryKey` | Create key |
| `ListTelemetryKeys` | List keys |
| `GetTelemetryKey` | Get key |
| `SetTelemetryKeyActive` | Enable/disable key |
| `DeleteTelemetryKey` | Delete key |
| `SetDataConnection` | Data export connection |
| `GetDataConnection` | Get connection |
| `ListDataConnections` | List connections |
| `SetDataConnectionActive` | Activate connection |
| `DeleteDataConnection` | Delete connection |

---

## Multiplayer (PlayFab-hosted servers) — `/Multiplayer/`

Use only if the title uses **PlayFab Multiplayer Servers**, not Photon-only hosting.

### Live session ops

| Operation | Purpose |
|-----------|---------|
| `ListMultiplayerServers` | Active sessions |
| `GetMultiplayerServerDetails` | Session details |
| `ListArchivedMultiplayerServers` | Archived sessions |
| `GetMultiplayerServerLogs` | Logs after terminate |
| `GetMultiplayerSessionLogsBySessionId` | Logs by session ID |
| `ShutdownMultiplayerServer` | Force shutdown |
| `RequestMultiplayerServer` | Request session |

### Build / infra (devops)

`CreateBuild*`, `DeleteBuild`, `ListBuildSummariesV2`, `ListVirtualMachineSummaries`, asset/certificate/secret upload, QoS, Party, quota APIs, etc. Full list: [Multiplayer Server](https://learn.microsoft.com/en-us/rest/api/playfab/multiplayer/multiplayer-server/?view=playfab-rest).

---

## Analytics — Insights & Data Connections

| Surface | Usage |
|---------|--------|
| **Insights KQL** | `POST https://insights.playfab.com/v1/rest/query` — query tables such as `events.all` |
| **Data Connections** | [Export to ADX / external stores](https://learn.microsoft.com/en-us/gaming/playfab/data-analytics/export-data/data-connection-adx) |
| **Admin `GetDataReport`** | Downloadable reports |

Insights management is being retired in favor of Data Connections; plan new work on Data Connections.

---

## Optional PlayFab APIs

| API | When |
|-----|------|
| [Groups](https://learn.microsoft.com/en-us/rest/api/playfab/groups/?view=playfab-rest) | Clans / guilds |
| [Multiplayer Matchmaking](https://learn.microsoft.com/en-us/rest/api/playfab/multiplayer/matchmaking?view=playfab-rest) | PlayFab queues (not Photon rooms) |
| [Client API](https://learn.microsoft.com/en-us/rest/api/playfab/client/?view=playfab-rest) | Avoid for bots; use Server/Admin |
| [Experimentation](https://learn.microsoft.com/en-us/rest/api/playfab/experimentation/?view=playfab-rest) | A/B tests |
| [Localization](https://learn.microsoft.com/en-us/rest/api/playfab/localization/?view=playfab-rest) | Localized cosmetic strings |

---

# Photon

## Realtime webhooks (inbound to your backend)

Configure in Photon Dashboard: **BaseUrl** + path suffixes.

| Path | When | Mod / live use |
|------|------|----------------|
| `PathCreate` | Room created / reload | Room registry |
| `PathClose` | Room destroyed | Session end |
| **`PathBeforeJoin`** | Join attempt | **Block join (ban, capacity)** |
| `PathJoin` | Player joined | Presence, audit |
| `PathLeave` | Player left | Presence |
| `PathEvent` | Event with `HttpForward` | Custom game events |
| `PathGameProperties` | Property with `HttpForward` | Room/player props |

Common fields: `AppId`, `AppVersion`, `Region`, `GameId`, `UserId`, `NickName`, `ActorNr`, `Type`.

Successful responses typically: `{ "ResultCode": 0 }`.

---

## Counter API (outbound GET)

Base: `https://counter.api.photonengine.net/`

Requires Premium Cloud, Circle, or Enterprise + dashboard token.

| Template | Meaning |
|----------|---------|
| `ccu` | Connected users (lobby + rooms) |
| `ccuroom` | Average CCU per room |
| `rooms` | Open rooms |
| `channels` | Chat channels |
| `messages` | Messages/sec (app-wide) |
| `messagesperroom` | Messages/sec per room |
| `messagesperchannel` | Messages/sec per channel |
| `disconnectratepeer` | Non-client disconnects/hour/peer |
| `messagrejects` | CCU cap rejections |
| `bandwidth` | Bytes/sec in+out |
| `bandwidthpeergame` | Game server traffic/peer |
| `bandwidthpeermaster` | Master server traffic/peer |
| `bandwidthpeerwebhook` | Webhook traffic/peer |
| `bandwidthpeer` | Total traffic/peer |
| `traffic` | Total bytes (range) |

Query patterns:

- `/query_total/app/{appId}/{template}?start=&end=&by=region`
- `/query_range/app/{appId}/{template}?start=&end=&step=`
- `/label_values/app/{appId}/region?start=&end=`

Self-hosted license: `/query_total/lic/{hash}/licccu`, `/query_range/lic/{hash}/licservers`.

---

## WebRPC

Uses the same **BaseUrl** as webhooks. Clients invoke HTTP methods on your backend through Photon. Useful for in-game actions, not for Discord polling live rosters.

---

## Headless Realtime client (SDK, not REST)

For live room lists and in-room player dictionaries:

1. Connect to Master Server → lobby / `RoomInfo` list (name, player count, custom properties).
2. Optionally join room → read `Room.Players` and custom player properties.

Run as a separate small service (e.g. .NET Photon SDK), not necessarily inside the Discord bot process.

---

## Suggested Discord command → backend mapping

| Command idea | Backend |
|--------------|---------|
| `/player lookup` | `GetPlayFabIDsFromServerCustomIDs` → `GetPlayerCombinedInfo` or `ExecuteFunction` |
| `/player inventory` | `Inventory/GetInventoryItems` + `Catalog/GetItems` |
| `/player ban` | `Admin/BanUsers` |
| `/player unban` | `Admin/RevokeBans` or `RevokeAllBansForUser` |
| `/online` | Your presence store, or Counter API `ccu` (title-wide only) |
| `/rooms` | Webhook-maintained DB or headless Photon lobby client |
| `/stats live` | Counter API `ccu`, `rooms` |

---

*Generated as a project reference for PlayFab + Photon Discord tooling. Trim to the endpoints your title actually uses once Economy v1/v2 and ID mapping are confirmed.*
