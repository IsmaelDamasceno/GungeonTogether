# GungeonNearby — Network Architecture

## Overview

Host-client topology. The **host** runs all authoritative game logic (enemy AI, HP, dungeon generation). **Clients** hold proxy/ghost objects that mirror host state. Client-side prediction is used for hit detection only — damage is always confirmed by the host.

---

## Transport Layer

```mermaid
graph TD
    NM[NetworkManager]
    IT[ITransport]
    ST[SteamTransport]
    LT[LanTransport]
    TF[TransportFactory<br/>reads --gt-lan-port arg]

    NM -->|InitialiseSteam| ST
    NM -->|InitialiseLan port| LT
    ST -->|implements| IT
    LT -->|implements| IT
    NM -->|holds| IT
    TF -.->|cmd-line shortcut| LT
```

`LanTransport` uses **TCP** for reliable packets (handshake, damage) and **UDP** for unreliable packets (position). Background threads receive into a queue; `Update()` dispatches on the Unity main thread.

---

## Session Handshake

```mermaid
sequenceDiagram
    participant C as Client
    participant H as Host

    C->>H: ConnectionRequest (ClientId, ProtocolVersion)
    H->>H: HandleJoinRequest → assign ID (2, 3, 4…)
    H->>C: ConnectionAccepted (HostId=1, AssignedId=N, ProtocolVersion)
    C->>C: LocalPlayerId = AssignedId
    Note over H,C: Session established
    C->>H: InstancePayload (NetworkId, reliability chosen by sender)
    H->>H: Dispatch to registry + relay to other clients
```

Host is always player ID **1**. Each joining client gets a sequentially assigned ID starting at **2**.

---

## Packet Flow (runtime)

```mermaid
flowchart LR
    subgraph Client
        CU[ClientController.Update]
        CS[Send PlayerPosition\nunreliable UDP]
        CH[HandleConnectionAccepted]
    end

    subgraph Host
        HU[HostController.Update\nenemy AI, spawns]
        HR[HandlePlayerPosition\nbroadcast to others]
        HJ[HandleJoinRequest\nassign ID]
    end

    subgraph Transport
        direction TB
        TCP[TCP\nreliable]
        UDP[UDP\nunreliable]
    end

    CU --> CS --> UDP --> HR
    CH --> CU
    HJ --> TCP --> CH
```

---

## Hit Detection (designed, not yet implemented)

```mermaid
sequenceDiagram
    participant C as Client
    participant H as Host
    participant OC as Other Clients

    Note over C: Bullet hits enemy locally
    C->>C: Visual feedback immediately
    C->>H: HitRequest (enemyId, damage, timestamp)
    H->>H: Validate + subtract HP
    H->>OC: EnemyState (enemyId, newHP, position)
    H->>C: EnemyState (confirmed)
```

Client-side prediction: if a bullet hits on YOUR screen, you report it. Host is authoritative on HP. Same rule applies in reverse — you only take damage if the bullet hits you on your own screen, then report to host.

---

## Proxy / Network Object Registry

```mermaid
classDiagram
    class INetworkPacket {
        +PacketType Type
        +ulong NetworkId
    }
    class INetworkProxy {
        +ulong NetworkId
        +OnSpawned(INetworkPacket)
        +HandlePacket(INetworkPacket)
        +OnDespawned()
        +Update()
    }
    class EnemyProxy
    class PlayerProxy
    class ProjectileProxy
    class NetworkObjectRegistry {
        +Register(proxy)
        +Unregister(id)
        +Dispatch(id, packet)
        +Update()
    }
    class NetworkManager {
        +ProcessPacket(senderId, packet)
    }

    INetworkProxy <|-- EnemyProxy
    INetworkProxy <|-- PlayerProxy
    INetworkProxy <|-- ProjectileProxy
    NetworkObjectRegistry --> INetworkProxy
    NetworkManager --> NetworkObjectRegistry : Dispatch by NetworkId
```

Each networked packet carries a **NetworkId** (0 = system packet, non-zero = entity packet). `NetworkManager` routes entity packets to the registry by `NetworkId` without inspecting the payload. The registered proxy receives the packet and casts to its own type. If no proxy is registered for a given `NetworkId`, an error is logged — spawning is the game layer's responsibility, not the network layer's.

---

## Dungeon Sync (designed, not yet implemented)

Host sends two seeds to clients before floor generation begins. Generation is deterministic given the same seeds, so no further map data needs to be transmitted.

```mermaid
sequenceDiagram
    participant H as Host
    participant C as Client

    H->>C: DungeonSeedPacket (seed1, seed2)
    C->>C: Generate floor with same seeds
    Note over H,C: Floors are identical
```

---

## What is implemented today

| Feature | Status |
|---|---|
| ITransport abstraction | ✅ |
| SteamTransport (P2P via reflection) | ✅ |
| LanTransport (TCP + UDP) | ✅ |
| UGUI overlay panel (Main / Steam / LAN) | ✅ |
| Session handshake (ConnectionRequest / Accepted) | ✅ |
| Host-assigned player IDs | ✅ |
| InstancePayload packets routed by NetworkId | ✅ |
| Host relay to other clients | ✅ |
| Ghost player GameObjects + interpolation | ❌ |
| Network object registry / proxy system | ✅ |
| Hit request / host-authoritative HP | ❌ |
| Enemy sync | ❌ |
| Dungeon seed sync | ❌ |
| SessionStatePacket (late-join state dump) | ❌ |
