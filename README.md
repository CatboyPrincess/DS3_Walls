# DS3_Walls
dark souls 3 walls and radar or whatever

## Resources Used

* https://www.youtube.com/watch?v=t1ErGj0YnaM
* https://github.com/erfg12/memory.dll

## Current Issues

* weird blackness on opening process
* font colour is weird for text
* doesn't use camera angle, uses char angle to perform rotations
* update frequency is restricted seemingly, might be machine dependent. That is to say, frequencies higher than 1 Hz will cause mini-stutters (probably has something to do with the default rendering system)

## Future Considerations

* use SFML or SDL or some other rendering system that doesn't cause DS3 to mini-stutter
* improve user customization (choosing colours, etc.)
* display farthest distance displayed by the minimap to help user gauge distance
* more compact enemy player height info (ex. font colour changes with height difference)
