```plantuml
participant Sender as Se
participant Saga as S
database Database as DB
Se --> S: ""CreateOrder""

S --> S: ""RequestTimeout""

note right
  User is given 1o minutes
  to cancel the order before
  order is confirmed
end note

alt User cancels order
Se --> S: ""CancelOrder""
S --> DB: Cancel order
S --> Se: Order cancelled

else Grace period expires 

[--> S: ""Timeout<BuyerRemorse>""
S --> DB: Confirm order
S --> Se: Order confirmed
end
```

```plantuml
[*] -[#green,bold]-> WaitingForConfirmation : ""CreateOrder""
' WaitingForConfirmation --> WaitingForConfirmation : ""Timeout<BuyerRemorse>""

WaitingForConfirmation -[#green,bold]-> ConfirmOrder : ""Timeout<BuyerRemorse>""

note on link
  If the user does not cancel 
  the order within 1 minute, 
  the order is automatically 
  confirmed
end note

WaitingForConfirmation -[#green,bold]-> CancelOrder : ""CancelOrder""

note on link
  The user has 1 minute  
  to cancel the order
end note


ConfirmOrder --> [*]
CancelOrder --> [*]
```

## How NSB persistence and EF interact

We must reuse the connection from the persistence, otherwise the transaction is promoted to a distributed one.

**TODO** What happens if we take persistence out of the equation? e.g. create a new endpoint which processes an event from the saga, and check if we can use the connection from the transport directly, or we still need to do some hacks to get it work. 
