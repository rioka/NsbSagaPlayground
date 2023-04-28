```plantuml
participant Saga as S
[--> S: ""CreateOrder""

S --> S: ""RequestTimeout""

alt User cancels order
[--> S: ""CancelOrder""
S --> S: Cancel order

else Grace period expires 

[--> S: ""Timeout<BuyerRemorse>""
S --> S: Confirm order
end
```

```plantuml
[*] -[#green,bold]-> WaitingForConfirmation : ""CreateOrder""
' WaitingForConfirmation --> WaitingForConfirmation : ""Timeout<BuyerRemorse>""

WaitingForConfirmation -[#green,bold]-> ConfirmOrder : ""Timeout<BuyerRemorse>""

WaitingForConfirmation -[#green,bold]-> CancelOrder : ""CancelOrder""

ConfirmOrder --> [*]
CancelOrder --> [*]
```

## How NSB persistence and EF interact

We must reuse the connection from the persistence, otherwise the transaction is promoted to a distributed one.

**TODO** What happens if we take persistence out of the equation? e.g. create a new endpoint which processes an event from the saga, and check if we can use the connection from the transport directly, or we still need to do some hacks to get it work. 
