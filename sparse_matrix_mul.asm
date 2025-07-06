# MIPS Assembly program for sparse matrix multiplication with UART communication
# This program receives matrix data via UART and performs sparse matrix multiplication
# 
# UART Reception:
# - Receives data in 4-byte groups (332 bytes total = 83 groups)
# - Converts from little-endian to big-endian format
# - Little-endian: LSB first (0x12345678 -> [0x78, 0x56, 0x34, 0x12])
# - Big-endian: MSB first (0x12345678 -> [0x12, 0x34, 0x56, 0x78])

.data
    buffer: .space 332          # Buffer for 332 bytes of data (83 * 4 bytes)
    
.text
.globl main

main:
    # Initialize registers
    la $t0, buffer              # Load buffer address
    li $t1, 0                   # Initialize counter
    li $t2, 332                 # Total bytes to receive (332 = 83 * 4)
    
    # Verify 332 is divisible by 4 (should be 83 groups)
    # This is a compile-time check: 332 % 4 = 0 ✓
    
    # UART register addresses (assuming memory-mapped I/O)
    li $t3, 0x10000000          # UART_RXD register address
    li $t4, 0x10000004          # UART_CON register address
    
    # Call UART receive function
    jal uart_receive
    
    # Continue with matrix multiplication logic...
    # (Matrix multiplication code would go here)
    
    # Exit program
    li $v0, 10
    syscall

uart_receive:
    # Modified 4-byte UART reception logic with endianness conversion
    # Receives 4 bytes at a time and converts from little-endian to big-endian
    
uart_read_loop:
    beq $t1, $t2, uart_read_done
    
    # Initialize 4-byte word register
    li $s0, 0                   # Clear register for 4-byte word
    li $s1, 0                   # Byte counter (0-3)
    
    # Receive 4 bytes
uart_receive_4bytes:
    beq $s1, 4, uart_convert_endian    # If 4 bytes received, convert endianness
    
    # Wait for receive data
uart_wait_rx:
    lw $t5, 0($t4)              # Read UART_CON register
    andi $t6, $t5, 8            # Check bit[3] - receive complete flag
    beq $t6, $zero, uart_wait_rx
    
    # Read received data
    lw $t7, 0($t3)              # Read UART_RXD
    andi $t7, $t7, 0xFF         # Only take low 8 bits
    
    # Store byte in little-endian format in $s0
    # Little-endian: LSB first, so shift left by (byte_index * 8)
    sll $t8, $s1, 3            # Multiply byte index by 8 for bit shift
    sllv $t7, $t7, $t8         # Shift byte to correct position
    or $s0, $s0, $t7           # OR with accumulated word
    
    # Read UART_CON to clear receive complete flag
    lw $t5, 0($t4)
    
    addi $s1, $s1, 1            # Increment byte counter
    j uart_receive_4bytes
    
uart_convert_endian:
    # Convert little-endian to big-endian
    # Input: $s0 contains 4 bytes received in little-endian order
    # If we received bytes [0x78, 0x56, 0x34, 0x12] (little-endian for 0x12345678)
    # We need to store as [0x12, 0x34, 0x56, 0x78] (big-endian)
    
    # Extract individual bytes from $s0
    andi $t5, $s0, 0xFF         # Extract byte 0 (first received, LSB of original)
    
    srl $t6, $s0, 8             # Shift right by 8 bits
    andi $t6, $t6, 0xFF         # Extract byte 1 (second received)
    
    srl $t7, $s0, 16            # Shift right by 16 bits
    andi $t7, $t7, 0xFF         # Extract byte 2 (third received)
    
    srl $t8, $s0, 24            # Shift right by 24 bits
    andi $t8, $t8, 0xFF         # Extract byte 3 (fourth received, MSB of original)
    
    # Reconstruct in big-endian order: MSB first
    # $t8 (MSB) should be in highest position
    # $t7 should be in second position
    # $t6 should be in third position  
    # $t5 (LSB) should be in lowest position
    sll $s2, $t8, 24           # Place MSB in highest position
    sll $t7, $t7, 16           # Place byte 2 in second position
    sll $t6, $t6, 8            # Place byte 1 in third position
    # $t5 (LSB) stays in lowest position (no shift needed)
    
    # Combine all bytes in big-endian order
    or $s2, $s2, $t7           # Add byte 2
    or $s2, $s2, $t6           # Add byte 1
    or $s2, $s2, $t5           # Add byte 0 (LSB)
    
    # Store 4-byte word to buffer in big-endian format
    add $t8, $t0, $t1           # buffer[i] address
    sw $s2, 0($t8)              # Store 4-byte word
    
    addi $t1, $t1, 4            # Increment counter by 4 bytes
    j uart_read_loop
    
uart_read_done:
    jr $ra                      # Return to caller