Utils contains functions that can be useful anywhere in the code. 
New functions can be added to this class at any time if wanted. The purpose is to minimize duplication in the code by adding same funktionality at different code parts.

There are the following #regions in the code:

#Vectors:
Vector based funktions.
Contains functions for easier handling of vectors.
These can be functions that project the vector onto a specific Y value or move a point by a desired distance in a desired direction and much more.

#Comparer
Contains functions that should ensure a problem-free and easy comparison between data.
Examples are AlmostEqual() and AlmostEqualOrLess().
These functions are designed to compare whether 2 floating-point numbers contain the same value up to a specified precision or, in the case of the latter, one of the numbers is smaller than the other.
Functions that compare other data types than numeric values can also be added if the need exist.

#Usfull Algorithms
Provides a collection of small algorithms that should simplify the processing of data.

#EmptyObjects
Functions to create one or multiple new game objects with specific components during runtime

#ChildObjects
Functions to access and/or manipulate the child objects and their child objects and so on of a given Gameobjects

#Helper
Helpful functions that cannot be clearly divided into categories such as rotating an object in a smooth motion instead of "snapping in the direction"