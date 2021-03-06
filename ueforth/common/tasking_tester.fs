( Trying some things with tasks )

: printer1 begin 42 emit 1000 ms again ;
: printer2 begin 43 emit 500 ms again ;
: runner begin pause again ;

' printer1 1000 1000 task print1
' printer2 1000 1000 task print2
print1 start-task
print2 start-task
runner
