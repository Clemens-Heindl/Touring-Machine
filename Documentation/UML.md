@startuml

class User {
  userID
  email
  username
  password
}

class Tour {
  tourID
  name
  tourDescription
  from
  to
  transportType
  tourDistance
  estimatedTime
  routeInformation
  createdBy : User
}

class TourLog {
  logID
  dateTime
  comment
  difficulty
  totalDistance
  totalTime
  rating
  createdBy : User
  relatedTour : Tour
}

' Relationships
User "1" <-- "0..*" Tour : createdBy
User "1" <-- "0..*" TourLog : createdBy
Tour "1" <-- "0..*" TourLog : relatedTour

@enduml
