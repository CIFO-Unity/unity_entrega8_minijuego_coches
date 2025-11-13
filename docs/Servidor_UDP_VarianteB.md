**Servidor UDP para carrera de coches — Variante B (recomendado: posición + velocidad + inputs)

Resumen
-------
Documento de diseño mínimo y práctico para un servidor UDP (Java) que soporte partidas de 2–4 jugadores en una carrera de coches. Variante B añade velocidad e inputs al paquete de estado para mejorar la predicción y suavizado en clientes.

Decisiones clave
----------------
- Protocolo: UDP (puerto 6001).
- Formato: binario, little-endian, compacto.
- Implementación server: Java (un único archivo `SimpleUdpServer.java` en la siguiente fase).
- Modelo: servidor relay (recibe estados de clientes y reenvía snapshots consolidados a todos los clientes a una tasa fija).

Parámetros por defecto
----------------------
- Puerto: `6001` (UDP)
- Broadcast server -> clients: `20 Hz` (cada 50 ms)
- Client -> server send rate: `20 Hz` (recomendado)
- Timeout cliente: `5 s`
- Máx jugadores: `4`

Formato de mensajes (little-endian)
-----------------------------------
Todos los mensajes comienzan con 1 byte `type`.

Tipos (1 byte)
- `0x01` = Join (cliente -> servidor)
- `0x02` = JoinAck (servidor -> cliente)
- `0x03` = StateUpdate (cliente -> servidor)
- `0x04` = ServerSnapshot (servidor -> clientes)
- `0x05` = Disconnect (cliente -> servidor)

1) Join (cliente -> servidor)
- [1 byte] type = 0x01
- Opcional: nombre/metadata (omitible)

2) JoinAck (servidor -> cliente)
- [1 byte] type = 0x02
- [1 byte] clientId (1..4)

3) StateUpdate (cliente -> servidor) — Variante B (recomendada)
- [1 byte] type = 0x03
- [1 byte] clientId (0 si aún no tiene id)
- [4 bytes] seq (uint32) — secuencia incremental por cliente
- [4 bytes] timestampMs (uint32) — reloj local del cliente en ms (opcional pero recomendado)
- [12 bytes] position (float x,y,z)
- [4 bytes] yaw (float) — rotación en Y (puedes usar quaternion si prefieres)
- [12 bytes] linearVelocity (float vx,vy,vz)
- [4 bytes] steer (float) — rango [-1..1]
- [4 bytes] throttle (float) — rango [0..1] o [-1..1] si tiene reversa
- [4 bytes] brake (float) — rango [0..1]

Total aproximado: 1+1+4+4+12+4+12+4+4+4 = 46 bytes

Notas:
- Se usan floats IEEE-754 (32-bit).
- Endianness: little-endian. En Java: `ByteBuffer.order(ByteOrder.LITTLE_ENDIAN)`.
- `seq` permite descartar paquetes antiguos y detectar reordenamientos.
- `timestampMs` ayuda para interpolación y diagnósticos.

4) ServerSnapshot (servidor -> clientes)
- [1 byte] type = 0x04
- [1 byte] playersCount P
- Repetir P veces:
  - [1 byte] clientId
  - [4 bytes] seq (última secuencia vista del cliente)
  - [4 bytes] timestampMs (último timestamp del cliente)
  - [12 bytes] position (3 floats)
  - [4 bytes] yaw (float)
  - [12 bytes] linearVelocity (3 floats)
  - [4 bytes] flags o padding (opcional)

Tamaño por jugador en snapshot: ~37 bytes; para 4 jugadores: snapshot total ~1+1+4*37 = ~150 bytes (seguirá siendo pequeño).

5) Disconnect
- [1 byte] type = 0x05
- [1 byte] clientId

Comportamiento del servidor (resumen)
-------------------------------------
- Abrir `DatagramSocket` en `0.0.0.0:6001`.
- Mantener estructura de clientes:
  - `clientId` (1..4), `InetSocketAddress endpoint`, `lastSeen` timestamp, `lastSeq`, `pos`, `yaw`, `vel`, `flags`.
- Asignación de ID: al recibir `Join`, asignar el primer `clientId` libre y responder con `JoinAck`.
- Recepción: loop que recibe `DatagramPacket`, parsea `type` y actualiza estado:
  - `StateUpdate`: si `seq` > `lastSeq` para ese cliente -> actualizar estado y `lastSeen`.
- Broadcast: cada 50 ms, construir `ServerSnapshot` con todos los clientes activos y enviar a cada `endpoint` registrado.
- Timeout: remover cliente si `now - lastSeen > 5s`.
- Concurrency: usar `ConcurrentHashMap` o sincronización para proteger accesos entre thread de recepción y thread programado de broadcast.

Recomendaciones de implementación en Java
-----------------------------------------
- Sockets: `DatagramSocket socket = new DatagramSocket(6001);`
- Recepción: reservar buffer de recv de 512 bytes (suficiente).
- Serialización: `ByteBuffer bb = ByteBuffer.allocate(size).order(ByteOrder.LITTLE_ENDIAN);`
- Data structures: `Map<Integer, ClientInfo>` y `Map<SocketAddress, Integer>` para mapear endpoints a clientId; `ClientInfo` contiene endpoint y estado.
- Threads:
  - Thread principal o Executor para `socket.receive(packet)` (bloqueante).
  - `ScheduledExecutorService` para `broadcastSnapshot()` cada 50 ms.
- Logging mínimo: `System.out.printf` para joins/disconnects y conteo de clientes.

Integración cliente (Unity) — notas prácticas
---------------------------------------------
- Cliente debe:
  1. En startup enviar `Join` y esperar `JoinAck` para obtener `clientId`.
  2. En un `FixedUpdate` a la tasa deseada (ej. 20 Hz) enviar `StateUpdate` con `seq++`, `timestampMs = (uint) (DateTime.UtcNow - epoch)`, posición `transform.position`, `yaw = transform.rotation.eulerAngles.y`, `linearVelocity` (obtenida del `Rigidbody.velocity` o calculada), y `inputs` (steer, throttle, brake).
  3. En proceso asíncrono recibir `ServerSnapshot` y actualizar estado de los otros jugadores.
- Interpolación: mantener buffer de snapshots por jugador y usar un `interpolationDelay` (ej. 100 ms) para reproducir suavemente. Usar `position` y `linearVelocity` para extrapolación si hace falta.
- Reconciliación: si el cliente aplica predicción local, cuando llegue snapshot con `seq` más nuevo, comparar y corregir suavemente.

Manejo de NAT / Firewall
------------------------
- Cliente detrás de NAT: normalmente puede enviar paquetes UDP a servidor público y recibir respuestas en el mismo endpoint; si hay problemas, considerar NAT punchthrough o usar servidor público con puerto abierto.
- En Windows: abrir puerto UDP 6001 o permitir la app Java en el Firewall.

Pruebas locales
---------------
- Prueba en `localhost` con múltiples instancias cliente apuntando a `127.0.0.1:6001`.
- Logs recomendados: mostrar joins, updates (seq), y snapshots enviados.

Seguridad y validación básica
----------------------------
- Validar rangos en servidor (e.g., posición dentro del circuito, velocidad razonable) para evitar cheats básicos.
- En producción, añadir autenticación en `Join` (token) y cifrado si necesario.

Optimización y compresión (opcional)
------------------------------------
- Si el ancho de banda es crítico, considerar:
  - Quantización de posiciones (e.g., int16 con escala)
  - Envío diferencial (solo cuando estado cambie más de un umbral)
  - Compresión simple (zlib) para snapshots grandes

Siguiente paso sugerido
-----------------------
Si estás conforme con Variante B, generaré `SimpleUdpServer.java` (único archivo, implementando el protocolo B) y un cliente stub para Unity (`UdpCarNetwork.cs`) que muestre cómo serializar/parsear los paquetes.

Contacto rápido
---------------
Si quieres modificar algo del protocolo (p. ej. usar quaternion en lugar de `yaw`, quitar `timestampMs`, o añadir `lapIndex`), dímelo ahora y actualizo el documento antes de generar el código.
