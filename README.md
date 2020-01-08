# NaughtyWaterBuoyancy
Water Buoyancy Simulation for Unity Engine

![gif](https://github.com/dbrizov/dbrizov.github.io/blob/master/images/project-images/water-buoyancy/idle.gif)
![gif](https://github.com/dbrizov/dbrizov.github.io/blob/master/images/project-images/water-buoyancy/dive-in.gif)
![screenshot](https://github.com/dbrizov/dbrizov.github.io/blob/master/images/project-images/water-buoyancy/surface-normal.png)

## System Requirements
Unity 2018.3.0 or later versions. Feel free to try older version

## How To Setup
You can use the ready to go prefabs (**WaterPro_DayTime** and **WaterPro_NightTime**). <br>
They are both located in the Prefabs folder. The only difference between the two
is that they use different shaders for daylight and moonlight. <br>

The two important scripts are **WaterVolume.cs** and **FloatingObject.cs**. <br>
The water volume applies buoyancy forces only to floating objects.
The water volume needs to have a tag **Water Volume** in order for the floating objects to detect collisions with the water.
The floating is just like in real life physics. If an object has density lesser than
the density of the water, it will float. If an object has greater density than that of the water, it will sink.
If for example an object has density equal to half the density of the water, then the object will submerge exactly half of its volume.
The other half of the volume will be above the water. For more info see this [Buoyancy Reference](http://scienceprimer.com/buoyancy) <br>

The **WaterVolume.cs** script works only with the procedural Mesh in the project due to performance optimizations.

## License
The MIT License (MIT)

Copyright (c) 2015 Denis Rizov

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
