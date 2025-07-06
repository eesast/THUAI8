# UART 4-Byte Reception with Endianness Conversion

## Overview
This implementation modifies the UART reception logic to receive data in 4-byte groups and convert from little-endian to big-endian format.

## Key Changes

### 1. Reception Mode
- **Before**: Byte-by-byte reception (1 byte at a time)
- **After**: 4-byte group reception (4 bytes at a time)

### 2. Endianness Conversion
- **Input**: Little-endian format (LSB first)
- **Output**: Big-endian format (MSB first)

### 3. Data Integrity
- **Total bytes**: 332 bytes (83 groups of 4 bytes)
- **Divisibility**: 332 ÷ 4 = 83 (perfect division)

## Endianness Conversion Logic

### Example:
If we want to represent the value `0x12345678`:

**Little-endian reception** (bytes received in this order):
1. First byte: `0x78` (LSB)
2. Second byte: `0x56`
3. Third byte: `0x34`
4. Fourth byte: `0x12` (MSB)

**Big-endian storage** (bytes stored in this order):
1. First byte: `0x12` (MSB)
2. Second byte: `0x34`
3. Third byte: `0x56`
4. Fourth byte: `0x78` (LSB)

### Algorithm:
1. Receive 4 bytes sequentially into a 32-bit register
2. Extract each byte using bit masks and shifts
3. Rearrange bytes in big-endian order
4. Store the converted 4-byte word to buffer

## Register Usage
- `$t0`: Buffer base address
- `$t1`: Byte counter (increments by 4)
- `$t2`: Total bytes to receive (332)
- `$t3`: UART_RXD register address
- `$t4`: UART_CON register address
- `$s0`: Temporary storage for 4-byte word
- `$s1`: Byte counter within 4-byte group
- `$s2`: Final converted 4-byte word

## Error Handling
- Maintains original UART status checking logic
- Preserves receive complete flag clearing
- Ensures all 332 bytes are received

## Testing
The implementation can be tested by:
1. Simulating UART input with known byte patterns
2. Verifying the endianness conversion produces expected results
3. Confirming total byte count remains 332

## Compatibility
- Maintains compatibility with existing matrix multiplication code
- Preserves buffer structure and addressing
- Keeps same total data size (332 bytes)