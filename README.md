# CS690-FinalProject
The system shall help users organize and track movies and TV shows they want to watch by storing watch lists and viewing progress in a simple and accessible way.
Use Cases (UC)
UC1: Add Watch Item
FR1: The system shall allow the user to add a movie or TV show to a watch list
FR2: The system shall store the title and type (movie or TV show)
UC2: View Watch List
FR3: The system shall display all saved watch items
FR4: The system shall show the status of each item (not started, in progress, completed)
UC3: Update Watching Progress
FR5: The system shall allow the user to update the last watched episode
FR6: The system shall allow the user to mark an item as completed
UC4: Resume Watching
FR7: The system shall display the last recorded progress for a selected item
FR8: The system shall allow the user to quickly identify where to resume watching
UC5: Remove Watch Item
FR9: The system shall allow the user to remove a watch item
FR10: The system shall update the watch list after removal
Non-Functional Requirements
NFR1 – Usability: The system shall be easy to use through a simple console-based interface. It is appropriate because Chloe needs a quick and distraction-free way to track shows without clutter.
NFR2 – Reliability: The system shall consistently save and retrieve watch list data without loss. It is appropriate because losing progress would defeat the purpose of tracking viewing history.
Constraints
The user interface shall be console-based
The system shall be developed using dotnet version 10
Prioritization
Use Cases
High: Add Watch Item, Update Watching Progress
Medium: View Watch List, Resume Watching
Low: Remove Watch Item
Functional Requirement Prioritization
High: FR1–FR6 (core tracking functionality)
Medium: FR7–FR8 (resuming viewing)
Low: FR9–FR10 (list maintenance)
Reason: Tracking and updating progress is critical before viewing or managing the list.
