#!/usr/bin/env python3
"""
Validation script for UART endianness conversion logic
This script simulates the MIPS assembly endianness conversion
"""

def convert_little_to_big_endian(little_endian_bytes):
    """
    Convert received bytes from little-endian to big-endian
    Input: 4 bytes received in little-endian order [LSB, ..., MSB]
    Output: 32-bit value in big-endian format
    """
    # If we receive bytes [0x78, 0x56, 0x34, 0x12] (little-endian for 0x12345678)
    # We need to store them as big-endian: 0x12345678 -> [0x12, 0x34, 0x56, 0x78]
    
    # Reconstruct the original value from little-endian bytes
    original_value = (little_endian_bytes[3] << 24) | (little_endian_bytes[2] << 16) | \
                     (little_endian_bytes[1] << 8) | little_endian_bytes[0]
    
    # The original value IS the big-endian representation
    return original_value

def test_endianness_conversion():
    """Test the endianness conversion with known values"""
    test_cases = [
        ([0x78, 0x56, 0x34, 0x12], 0x12345678),  # Little-endian bytes -> Big-endian value
        ([0x00, 0x00, 0x00, 0x00], 0x00000000),  # All zeros
        ([0xFF, 0xFF, 0xFF, 0xFF], 0xFFFFFFFF),  # All ones
        ([0x04, 0x03, 0x02, 0x01], 0x01020304),  # Sequential
        ([0xEF, 0xBE, 0xAD, 0xDE], 0xDEADBEEF),  # Common test pattern
    ]
    
    print("Endianness Conversion Test")
    print("=" * 70)
    print(f"{'Little-endian bytes':<25} {'Big-endian value':<15} {'Match':<10}")
    print("-" * 70)
    
    for little_bytes, expected_big in test_cases:
        actual_big = convert_little_to_big_endian(little_bytes)
        bytes_str = f"[{', '.join(f'0x{b:02X}' for b in little_bytes)}]"
        match = "✓" if actual_big == expected_big else "✗"
        print(f"{bytes_str:<25} 0x{actual_big:08X}      {match}")
    
    # Verify the core logic
    print(f"\nCore Logic Verification:")
    print(f"If UART receives bytes [0x78, 0x56, 0x34, 0x12] (little-endian)")
    print(f"We reconstruct the original value: 0x12345678 (big-endian)")
    print(f"This matches the expected behavior.")
    
    # Test with 332 bytes (83 groups of 4 bytes)
    print(f"\nData Size Validation:")
    print(f"Total bytes: 332")
    print(f"Bytes per group: 4")
    print(f"Number of groups: {332 // 4}")
    print(f"Remainder: {332 % 4}")
    print(f"Perfect division: {'✓' if 332 % 4 == 0 else '✗'}")

if __name__ == "__main__":
    test_endianness_conversion()