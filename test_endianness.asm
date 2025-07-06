# Simple test program to validate endianness conversion
# This program simulates the endianness conversion logic

.data
    test_input: .word 0x12345678    # Test input (little-endian representation)
    test_output: .word 0            # Test output storage
    newline: .asciiz "\n"
    
.text
.globl main

main:
    # Load test input
    lw $s0, test_input
    
    # Apply endianness conversion (same logic as in sparse_matrix_mul.asm)
    # Extract individual bytes from $s0
    andi $t5, $s0, 0xFF         # Extract byte 0 (LSB)
    
    srl $t6, $s0, 8             # Shift right by 8 bits
    andi $t6, $t6, 0xFF         # Extract byte 1
    
    srl $t7, $s0, 16            # Shift right by 16 bits
    andi $t7, $t7, 0xFF         # Extract byte 2
    
    srl $t8, $s0, 24            # Shift right by 24 bits
    andi $t8, $t8, 0xFF         # Extract byte 3 (MSB)
    
    # Reconstruct in big-endian order
    sll $s2, $t8, 24           # Place MSB in highest position
    sll $t7, $t7, 16           # Place byte 2 in second position
    sll $t6, $t6, 8            # Place byte 1 in third position
    # $t5 (LSB) stays in lowest position
    
    # Combine all bytes in big-endian order
    or $s2, $s2, $t7           # Add byte 2
    or $s2, $s2, $t6           # Add byte 1
    or $s2, $s2, $t5           # Add byte 0 (LSB)
    
    # Store result
    sw $s2, test_output
    
    # Print original value
    li $v0, 1
    lw $a0, test_input
    syscall
    
    # Print newline
    li $v0, 4
    la $a0, newline
    syscall
    
    # Print converted value
    li $v0, 1
    lw $a0, test_output
    syscall
    
    # Print newline
    li $v0, 4
    la $a0, newline
    syscall
    
    # Exit
    li $v0, 10
    syscall