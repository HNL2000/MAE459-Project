Run executable in /bin/Release/net5.0
Various features of the simulation can be configured in properties.txt ~in the same folder as the executable~
    - The names should be self explanatory
    - The reactor equations are "constant", "sine" and "parametric" (without quotes)
    - Area is the total area (i.e., one slot x number of slots)
    - Circumference is the total perimeter (i.e., one slot x number of slots)
    - Set reactor length to < 0 for determining max length based on max temperature, > 0 to use a fixed length and ignore maximum temperature
Output file "output.txt" will be created in the same folder as executable
    
Known issues:
    - Using larger numbers of cells leads to a stack overflow (tested up to 13500 is functional, 15000 is not)
    - Using lower numbers of cells (< 10) also causes issues 
