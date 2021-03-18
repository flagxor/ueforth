( Trying some things with tasks )

: printer1   42 emit 1000 ms ;
: printer2   43 emit 500 ms ;
: runner   begin pause again ;

' printer1 1000 1000 task print1
' printer2 1000 1000 task print2
print1 start-task
print2 start-task
runner
