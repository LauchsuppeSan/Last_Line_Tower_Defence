A Collection of classes to create parameters for other classes
For exampple if there is a class "Person" somewhere in the project you can here create the class "PersonSettings" to store a bunch of parameters for "Person" such like 
string name
int age
string adress

then (In "Person" class) you can just use
PersonSettings settings = new PersonSettings()

to keep a easy and clear code structure
An other benefit is that in the inspectorView for the "Person" class the settings will be shown in its own region what make it even clean in the inspector