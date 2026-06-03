# Semester Project SWEN2 Protocol

## UI and UX Design Choices
Our design is based on a MVVM Architecture using Angulars two way binding for the communication between View and View Model. 
The folder models contains the data models and structure for the MVVM model layer. 

The main Page consists of multiple components which are visually separated:
- Header:
    The Header currently contains only static content but we will implement a login and logout button in the future
- Navbar:
    The Navbar will allow the user to switch between multiple views such as a statistics page or a tour log view but for now only the main page is implemented.
- Tour components:
    The 3 tour components, tour list, tour details and tour form display tour data and allow the user to create or modify new/existing tours
- Tour Log components:
    The Tour logs contain a bit less information than the tours especially the space for the map is not needed which is why only 2 components one with a dynamic list and a form components for creating and modifying logs similarly to the tour form. 

For each of these components there is a Typescript component class representing the View Model part of the MVVM Architecture which communicates with the HTML View which the user can see and interact with. As already mentioned this is achieved via the Angular two way binding 

The backend is already partially implemented but to simply demonstrate the functionality of our frontend application without the needed connection to the API, we included a bit of hard coded mock data, which consists of a few already existing tours, corresponding tour logs and hard coded images.

Our next steps in development are improving the backend and also the input forms for creating and modifying tours as well as tour logs. 