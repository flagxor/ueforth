( EForth High Level Definitions )

( Variables and User Variables )

: doVAR ( -- a ) R> ;

VARIABLE UP ( -- a, Pointer to the user area.)

: doUSER ( -- a, Run time routine for user variables.)
    R> @ \ retrieve user area offset
    UP @ + ; \ add to user area base addr
: doVOC ( -- ) R> CONTEXT ! ;
: FORTH ( -- ) doVOC [ 0 , 0 ,
: doUSER ( -- a ) R> @ UP @ + ;

( User Variables )

SP0 ( -- a, pointer to bottom of the data stack.)
RP0 ( -- a, pointer to bottom of the return stack.)
'?KEY ( -- a, execution vector of ?KEY. Default to ?rx.)
'EMIT ( -- a, execution vector of EMIT. Default to tx!)
'EXPECT ( -- a, execution vector of EXPECT. Default to 'accept'.)
'TAP ( -- a, execution vector of TAP. Defulat the kTAP.)
'ECHO ( -- a, execution vector of ECHO. Default to tx!.)
'PROMPT ( -- a, execution vector of PROMPT. Default to '.ok'.)
BASE ( -- a,.radix base for numeric I/O. Default to 10.)
tmp ( -- a, a temporary storage location used in parse and find.)
SPAN ( -- a, hold character count received by EXPECT.)
>IN ( -- a, hold the character pointer while parsing input stream.)
#TIB ( -- a, hold the current count and address of the terminal input buffer.
          Terminal Input Buffer used one cell after #TIB.)
CSP ( -- a, hold the stack pointer for error checking.)
'EVAL ( -- a, execution vector of EVAL. Default to EVAL.)
'NUMBER ( -- a, address of number conversion. Default to NUMBER?.)
HLD ( -- a, hold a pointer in building a numeric output string.)
HANDLER ( -- a, hold the return stack pointer for error handling.)
CONTEXT ( -- a, a area to specify vocabulary search order. Default to FORTH.
             Vocabulary stack, 8 cells follwing CONTEXT.)
CURRENT ( -- a, point to the vocabulary to be extended. Default to FORTH.
             Vocabulary link uses one cell after CURRENT.)
CP ( -- a, point to the top of the code dictionary.)
NP ( -- a, point to the bottom of the name dictionary.)
LAST ( -- a, point to the last name in the name dictionary.)

( Common Functions )

: ?DUP ( w -- w w | 0 ) DUP IF DUP THEN ;
: ROT ( w1 w2 w3 -- w2 w3 w1 ) >R SWAP R> SWAP ;
: 2DROP ( w w -- ) DROP DROP ;
: 2DUP ( w1 w2 -- w1 w2 w1 w2 ) OVER OVER ;
: + ( w w -- w ) UM+ DROP ;
: NOT ( w -- w ) -1 XOR ;
: NEGATE ( n -- -n ) NOT 1 + ;
: DNEGATE ( d -- -d ) NOT >R NOT 1 UM+ R> + ;
: D+ ( d d -- d ) >R SWAP >R UM+ R> R> + + ;
: - ( w w -- w ) NEGATE + ;
: ABS ( n -- +n ) DUP 0< IF NEGATE THEN ;

( Comparison )

: = ( w w -- t ) XOR IF 0 EXIT THEN -1 ;
: U< ( u u -- t ) 2DUP XOR 0< IF SWAP DROP 0< EXIT THEN - 0< ;
: < ( n n -- t ) 2DUP XOR 0< IF DROP 0< EXIT THEN - 0< ;
: MAX ( n n -- n ) 2DUP < IF SWAP THEN DROP ;
: MIN ( n n -- n ) 2DUP SWAP < IF SWAP THEN DROP ;
: WITHIN ( u ul uh -- t ) \ ul <= u < uh
         OVER - >R - R> U< ;

( Divide )

: UM/MOD ( ud u -- ur uq )
    2DUP U<
    IF NEGATE 15
        FOR >R DUP UM+ >R >R DUP UM+ R> + DUP
            R> R@ SWAP >R UM+ R> OR
            IF >R DROP 1 + R> ELSE DROP THEN R>
        NEXT DROP SWAP EXIT
    THEN DROP 2DROP -1 DUP ;
: M/MOD ( d n -- r q ) \ floored division
    DUP 0< DUP >R
    IF NEGATE >R DNEGATE R>
    THEN >R DUP 0< IF R@ + THEN R> UM/MOD R>
    IF SWAP NEGATE SWAP THEN ;
: /MOD ( n n -- r q ) OVER 0< SWAP M/MOD ;
: MOD ( n n -- r ) /MOD DROP ;
: / ( n n -- q ) /MOD SWAP DROP ;

( Multiply )

: UM* ( u u -- ud )
    0 SWAP ( u1 0 u2 ) 15
    FOR DUP UM+ >R >R DUP UM+ R> + R>
        IF >R OVER UM+ R> + THEN
    NEXT ROT DROP ;
: * ( n n -- n ) UM* DROP ;
: M* ( n n -- d )
    2DUP XOR 0< >R ABS SWAP ABS UM* R> IF DNEGATE THEN ;
: */MOD ( n n n -- r q ) >R M* R> M/MOD ;
: */ ( n n n -- q ) */MOD SWAP DROP ;

( Memory Alignment )

: CELL- ( a -- a ) -2 + ;
: CELL+ ( a -- a ) 2 + ;
: CELLS ( n -- n ) 2 * ;
: ALIGNED ( b -- a )
    DUP 0 2 UM/MOD DROP DUP
    IF 2 SWAP - THEN + ;
: BL ( -- 32 ) 32 ;
: >CHAR ( c -- c )
    $7F AND DUP 127 BL WITHIN IF DROP 95 THEN ;
: DEPTH ( -- n ) SP@ SP0 @ SWAP - 2 / ;
: PICK ( +n -- w ) 1 + CELLS SP@ + @ ;

( Memory Access )

: +! ( n a -- ) SWAP OVER @ + SWAP ! ;
: 2! ( d a -- ) SWAP OVER ! CELL+ ! ;
: 2@ ( a -- d ) DUP CELL+ @ SWAP @ ;
: COUNT ( b -- b +n ) DUP 1 + SWAP C@ ;
: HERE ( -- a ) CP @ ;
: PAD ( -- a ) HERE 80 + ;
: TIB ( -- a ) #TIB CELL+ @ ;
: @EXECUTE ( a -- ) @ ?DUP IF EXECUTE THEN ;
: CMOVE ( b b u -- )
    FOR AFT >R DUP C@ R@ C! 1 + R> 1 + THEN NEXT 2DROP ;
: FILL ( b u c -- )
    SWAP FOR SWAP AFT 2DUP C! 1 + THEN NEXT 2DROP ;
: -TRAILING ( b u -- b u )
    FOR AFT BL OVER R@ + C@ <
        IF R> 1 + EXIT THEN THEN
    NEXT 0 ;
: PACK$ ( b u a -- a ) \ null fill
    ALIGNED DUP >R OVER
    DUP 0 2 UM/MOD DROP
    - OVER + 0 SWAP ! 2DUP C! 1 + SWAP CMOVE R> ;

( Numeric Output )

: DIGIT ( u -- c ) 9 OVER < 7 AND + 48 + ;
: EXTRACT ( n base -- n c ) 0 SWAP UM/MOD SWAP DIGIT ;
: <# ( -- ) PAD HLD ! ;
: HOLD ( c -- ) HLD @ 1 - DUP HLD ! C! ;
: # ( u -- u ) BASE @ EXTRACT HOLD ;
: #S ( u -- 0 ) BEGIN # DUP WHILE REPEAT ;
: SIGN ( n -- ) 0< IF 45 HOLD THEN ;
: #> ( w -- b u ) DROP HLD @ PAD OVER - ;
: str ( n -- b u ) DUP >R ABS <# #S R> SIGN #> ;
: HEX ( -- ) 16 BASE ! ;
: DECIMAL ( -- ) 10 BASE ! ;

( Number Output )

: str ( n -- b u )
    ( Convert a signed integer to a numeric string.)
    DUP >R ( save a copy for sign)
    ABS ( use absolute of n)
    <# #S ( convert all digits)
    R> SIGN ( add sign from n)
    #> ; ( return number string addr and length)
: HEX ( -- )
    ( Use radix 16 as base for numeric conversions.)
    16 BASE ! ;
: DECIMAL ( -- )
    ( Use radix 10 as base for numeric conversions.)
    10 BASE ! ;
: .R ( n +n -- )
    ( Display an integer in a field of n columns, right justified.)
    >R str ( convert n to a number string)
    R> OVER - SPACES ( print leading spaces)
    TYPE ; ( print number in +n column format)
: U.R ( u +n -- )
    ( Display an unsigned integer in n column, right justified.)
    >R ( save column number)
    <# #S #> R> ( convert unsigned number)
    OVER - SPACES ( print leading spaces)
    TYPE ; ( print number in +n columns)
: U. ( u -- )
    ( Display an unsigned integer in free format.)
    <# #S #> ( convert unsigned number)
    SPACE ( print one leading space)
    TYPE ; ( print number)
: . ( w -- )
    ( Display an integer in free format, preceeded by a space.)
    BASE @ 10 XOR ( if not in decimal mode)
    IF U. EXIT THEN ( print unsigned number)
    str SPACE TYPE ; ( print signed number if decimal)
: ? ( a -- )
    ( Display the contents in a memory cell.)
    @ . ; ( very simple but useful command)

( Number Input )

: DIGIT? ( c base -- u t )
    >R 48 - 9 OVER <
    IF 7 - DUP 10 < OR THEN DUP R> U< ;
: NUMBER? ( a -- n T | a F )
    BASE @ >R 0 OVER COUNT ( a 0 b n)
    OVER C@ 36 =
    IF HEX SWAP 1 + SWAP 1 - THEN ( a 0 b' n')
        OVER C@ 45 = >R ( a 0 b n)
        SWAP R@ - SWAP R@ + ( a 0 b" n") ?DUP
        IF 1 - ( a 0 b n)
            FOR DUP >R C@ BASE @ DIGIT?
            WHILE SWAP BASE @ * + R> 1 +
            NEXT DROP R@ ( b ?sign) IF NEGATE THEN SWAP
        ELSE R> R> ( b index) 2DROP ( digit number) 2DROP 0
        THEN DUP
    THEN R> ( n ?sign) 2DROP R> BASE ! ;

( Basic I/O )

: ?KEY ( -- c T | F ) '?KEY @EXECUTE ;
: KEY ( -- c ) BEGIN ?KEY UNTIL ;
: EMIT ( c -- ) 'EMIT @EXECUTE ;
: NUF? ( -- f ) ?KEY DUP IF 2DROP KEY 13 = THEN ;
: PACE ( -- ) 11 EMIT ;
: SPACE ( -- ) BL EMIT ;
: CHARS ( +n c -- ) \ ???ANS conflict
    SWAP 0 MAX FOR AFT DUP EMIT THEN NEXT DROP ;
: SPACES ( +n -- ) BL CHARS ;
: TYPE ( b u -- ) FOR AFT DUP C@ EMIT 1 + THEN NEXT DROP ;
: CR ( -- ) 13 EMIT 10 EMIT ;
: do$ ( -- a )
    R> R@ R> COUNT + ALIGNED >R SWAP >R ;
: $"| ( -- a ) do$ ;
: ."| ( -- ) do$ COUNT TYPE ; COMPILE-ONLY
: .R ( n +n -- ) >R str R> OVER - SPACES TYPE ;
: U.R ( u +n -- ) >R <# #S #> R> OVER - SPACES TYPE ;
: U. ( u -- ) <# #S #> SPACE TYPE ;
: . ( n -- ) BASE @ 10 XOR IF U. EXIT THEN str SPACE TYPE ;
: ? ( a -- ) @ . ;

( Parsing )

: parse ( b u c -- b u delta ; <string> )
    tmp ! OVER >R DUP \ b u u
    IF 1 - tmp @ BL =
        IF \ b u' \ 'skip'
            FOR BL OVER C@ - 0< NOT WHILE 1 +
            NEXT ( b) R> DROP 0 DUP EXIT \ all delim
            THEN R>
        THEN OVER SWAP \ b' b' u' \ 'scan'
        FOR tmp @ OVER C@ - tmp @ BL =
           IF 0< THEN WHILE 1 +
           NEXT DUP >R ELSE R> DROP DUP 1 + >R
        THEN OVER - R> R> - EXIT
    THEN ( b u) OVER R> - ;
: PARSE ( c -- b u ; <string> )
    >R TIB >IN @ + #TIB @ >IN @ - R> parse >IN +! ;
: .( ( -- ) 41 PARSE TYPE ; IMMEDIATE
: ( ( -- ) 41 PARSE 2DROP ; IMMEDIATE
: \ ( -- ) #TIB @ >IN ! ; IMMEDIATE
: CHAR ( -- c ) BL PARSE DROP C@ ;
: TOKEN ( -- a ; <string> )
    BL PARSE 31 MIN NP @ OVER - CELL- PACK$ ;
: WORD ( c -- a ; <string> ) PARSE HERE PACK$ ;

( Dictionary Search )

: NAME> ( a -- xt ) CELL- CELL- @ ;
: SAME? ( a a u -- a a f \ -0+ )
    FOR AFT OVER R@ CELLS + @
      OVER R@ CELLS + @ - ?DUP
      IF R> DROP EXIT THEN THEN
    NEXT 0 ;
: find ( a va -- xt na | a F )
    SWAP \ va a
    DUP C@ 2 / tmp ! \ va a \ get cell count
    DUP @ >R \ va a \ count byte & 1st char
    CELL+ SWAP \ a' va
    BEGIN @ DUP \ a' na na
        IF DUP @ [ =MASK ] LITERAL AND R@ XOR \ ignore lexicon bits
        IF CELL+ -1 ELSE CELL+ tmp @ SAME? THEN
        ELSE R> DROP EXIT
        THEN
    WHILE CELL- CELL- \ a' la
    REPEAT R> DROP SWAP DROP CELL- DUP NAME> SWAP ;
: NAME? ( a -- xt na | a F )
    CONTEXT DUP 2@ XOR IF CELL- THEN >R \ context<>also
    BEGIN R> CELL+ DUP >R @ ?DUP
    WHILE find ?DUP
    UNTIL R> DROP EXIT THEN R> DROP 0 ;

( Terminal )

: ^H ( b b b -- b b b ) \ backspace
    >R OVER R> SWAP OVER XOR
    IF 8 'ECHO @EXECUTE
        32 'ECHO @EXECUTE \ distructive
        8 'ECHO @EXECUTE \ backspace
    THEN ;
: TAP ( bot eot cur c -- bot eot cur )
    DUP 'ECHO @EXECUTE OVER C! 1 + ;
: kTAP ( bot eot cur c -- bot eot cur )
    DUP 13 XOR
    IF 8 XOR IF BL TAP ELSE ^H THEN EXIT
    THEN DROP SWAP DROP DUP ;
: accept ( b u -- b u )
    OVER + OVER
    BEGIN 2DUP XOR
    WHILE KEY DUP BL - 95 U<
       IF TAP ELSE 'TAP @EXECUTE THEN
    REPEAT DROP OVER - ;
: EXPECT ( b u -- ) 'EXPECT @EXECUTE SPAN ! DROP ;
: QUERY ( -- )
    TIB 80 'EXPECT @EXECUTE #TIB ! DROP 0 >IN ! ;

( Error Handling )

: CATCH ( ca -- err#/0 )
    ( Execute word at ca and set up an error frame for it.)
    SP@ >R ( save current stack pointer on return stack )
    HANDLER @ >R ( save the handler pointer on return stack )
    RP@ HANDLER ! ( save the handler frame pointer in HANDLER )
    ( ca ) EXECUTE ( execute the assigned word over this safety net )
    R> HANDLER ! ( normal return from the executed word )
    ( restore HANDLER from the return stack )
    R> DROP ( discard the saved data stack pointer )
    0 ; ( push a no-error flag on data stack )
: THROW ( err# -- err# )
    ( Reset system to current local error frame an update error flag.)
    HANDLER @ RP! ( expose latest error handler frame on return stack)
    R> HANDLER ! ( restore previously saved error handler frame )
    R> SWAP >R ( retrieve the data stack pointer saved )
    SP! ( restore the data stack )
    DROP
    R> ; ( retrived err# )

CREATE NULL$ 0 , $," coyote"
: ABORT ( -- ) NULL$ THROW ;
: abort" ( f -- ) IF do$ THROW THEN do$ DROP ;

( Text Interpreter )

: $INTERPRET ( a -- )
    NAME? ?DUP
    IF @ $40 AND
        ABORT" compile ONLY" EXECUTE EXIT
    THEN 'NUMBER @EXECUTE IF EXIT THEN THROW ;
: [ ( -- ) doLIT $INTERPRET 'EVAL ! ; IMMEDIATE
: .OK ( -- ) doLIT $INTERPRET 'EVAL @ = IF ." ok" THEN CR ;
: ?STACK ( -- ) DEPTH 0< ABORT" underflow" ;
: EVAL ( -- )
    BEGIN TOKEN DUP C@
    WHILE 'EVAL @EXECUTE ?STACK
    REPEAT DROP 'PROMPT @EXECUTE ;

( Shell )

: PRESET ( -- ) SP0 @ SP! TIB #TIB CELL+ ! ;
: xio ( a a a -- ) \ reset 'EXPECT 'TAP 'ECHO 'PROMPT
    doLIT accept 'EXPECT 2! 'ECHO 2! ; COMPILE-ONLY
: FILE ( -- )
    doLIT PACE doLIT DROP doLIT kTAP xio ;
: HAND ( -- )
    doLIT .OK doLIT EMIT [ kTAP xio ;
    CREATE I/O ' ?RX , ' TX! , \ defaults
: CONSOLE ( -- ) I/O 2@ '?KEY 2! HAND ;

: QUIT ( -- )
    RP0 @ RP!
    BEGIN [COMPILE] [
        BEGIN QUERY doLIT EVAL CATCH ?DUP
        UNTIL 'PROMPT @ SWAP CONSOLE NULL$ OVER XOR
        IF CR #TIB 2@ TYPE
            CR >IN @ 94 CHARS
            CR COUNT TYPE ." ? "
        THEN doLIT .OK XOR
        IF $1B EMIT THEN
        PRESET
    AGAIN ;

( Interpreter and Compiler )

: [ ( -- )
    [ ' $INTERPRET ] LITERAL
    'EVAL ! ( vector EVAL to $INTERPRET )
; IMMEDIATE ( enter into text interpreter mode )

: ] ( -- )
    [ ' $COMPILE ] LITERAL
    'EVAL ! ( vector EVAL to $COMPILE )
;

( Primitive Compiler Words )

: ' ( -- xt ) TOKEN NAME? IF EXIT THEN THROW ;
: ALLOT ( n -- ) CP +! ;
: , ( w -- ) HERE DUP CELL+ CP ! ! ; \ ???ALIGNED
: [COMPILE] ( -- ; <string> ) ' , ; IMMEDIATE
: COMPILE ( -- ) R> DUP @ , CELL+ >R ;
: LITERAL ( w -- ) COMPILE doLIT , ; IMMEDIATE
: $," ( -- ) 34 WORD COUNT ALIGNED CP ! ;
: RECURSE ( -- ) LAST @ NAME> , ; IMMEDIATE

( Structures )

: <MARK ( -- a ) HERE ;
: <RESOLVE ( a -- ) , ;
: >MARK ( -- A ) HERE 0 , ;
: >RESOLVE ( A -- ) <MARK SWAP ! ;

: FOR ( -- a ) COMPILE >R <MARK ; IMMEDIATE
: BEGIN ( -- a ) <MARK ; IMMEDIATE
: NEXT ( a -- ) COMPILE next <RESOLVE ; IMMEDIATE
: UNTIL ( a -- ) COMPILE ?branch <RESOLVE ; IMMEDIATE
: AGAIN ( a -- ) COMPILE branch <RESOLVE ; IMMEDIATE
: IF ( -- A ) COMPILE ?branch >MARK ; IMMEDIATE
: AHEAD ( -- A ) COMPILE branch >MARK ; IMMEDIATE
: REPEAT ( A a -- ) [COMPILE] AGAIN >RESOLVE ; IMMEDIATE
: THEN ( A -- ) >RESOLVE ; IMMEDIATE
: AFT ( a -- a A ) DROP [COMPILE] AHEAD [COMPILE] BEGIN SWAP ; IMMEDIATE
: ELSE ( A -- A ) [COMPILE] AHEAD SWAP [COMPILE] THEN ; IMMEDIATE
: WHEN ( a A -- a A a ) [COMPILE] IF OVER ; IMMEDIATE
: WHILE ( a -- A a ) [COMPILE] IF SWAP ; IMMEDIATE
: ABORT" ( -- ; <string> ) COMPILE abort" $," ; IMMEDIATE
: $" ( -- ; <string> ) COMPILE $"| $," ; IMMEDIATE
: ." ( -- ; <string> ) COMPILE ."| $," ; IMMEDIATE

( Compiler )

: ?UNIQUE ( a -- a ) DUP NAME? IF ." reDef " OVER COUNT TYPE THEN DROP ;
: $,n ( a -- )
    DUP C@
    IF ?UNIQUE
        ( na) DUP LAST ! \ for OVERT
        ( na) HERE ALIGNED SWAP
        ( cp na) CELL-
        ( cp la) CURRENT @ @
        ( cp la na') OVER !
        ( cp la) CELL- DUP NP ! ( ptr) ! EXIT
    THEN $" name" THROW ;
.( FORTH Compiler )
: $COMPILE ( a -- )
    NAME? ?DUP
    IF @ $80 AND
        IF EXECUTE ELSE , THEN EXIT
    THEN 'NUMBER @EXECUTE
    IF [COMPILE] LITERAL EXIT
    THEN THROW ;
: OVERT ( -- ) LAST @ CURRENT @ ! ;
: ; ( -- )
    COMPILE EXIT [COMPILE] [ OVERT ; IMMEDIATE
: ] ( -- ) doLIT $COMPILE 'EVAL ! ;
: call, ( xt -- ) \ DTC 8086 relative call
    $E890 , HERE CELL+ - , ;
: : ( -- ; <string> ) TOKEN $,n doLIT doLIST call, ] ;
: IMMEDIATE ( -- ) $80 LAST @ @ OR LAST @ ! ;

( Defining Words )

: USER ( n -- ; <string> )
    TOKEN $,n OVERT
    doLIT doLIST COMPILE doUSER , ;
: CREATE ( -- ; <string> )
    TOKEN $,n OVERT
    doLIT doLIST COMPILE doVAR ;
: VARIABLE ( -- ; <string> ) CREATE 0 , ;

( Memory Dump )

: _TYPE ( b u -- )
    FOR AFT DUP C@ >CHAR EMIT 1 + THEN NEXT DROP ;
: dm+ ( b u -- b )
    OVER 4 U.R SPACE FOR AFT DUP C@ 3 U.R 1 + THEN NEXT ;
: DUMP ( b u -- )
    BASE @ >R HEX 16 /
    FOR CR 16 2DUP dm+ ROT ROT 2 SPACES _TYPE NUF? NOT WHILE
    NEXT ELSE R> DROP THEN DROP R> BASE ! ;

( Stack Tools )

: .S ( -- ) CR DEPTH FOR AFT R@ PICK . THEN NEXT ." <sp" ;
: .BASE ( -- ) BASE @ DECIMAL DUP . BASE ! ;
: .FREE ( -- ) CP 2@ - U. ;
: !CSP ( -- ) SP@ CSP ! ;
: ?CSP ( -- ) SP@ CSP @ XOR ABORT" stack depth" ;

( Dictionary Dump )

: >NAME ( xt -- na | F )
    CURRENT
    BEGIN CELL+ @ ?DUP WHILE 2DUP
        BEGIN @ DUP WHILE 2DUP NAME> XOR
        WHILE CELL-
        REPEAT THEN SWAP DROP ?DUP
    UNTIL SWAP DROP SWAP DROP EXIT THEN DROP 0 ;
: .ID ( a -- )
    ?DUP IF COUNT $01F AND _TYPE EXIT THEN ." {noName}" ;
: SEE ( -- ; <string> )
    ' CR CELL+
    BEGIN CELL+ DUP @ DUP IF >NAME THEN ?DUP
        IF SPACE .ID ELSE DUP @ U. THEN NUF?
    UNTIL DROP ;
: WORDS ( -- )
    CR CONTEXT @
    BEGIN @ ?DUP
    WHILE DUP SPACE .ID CELL- NUF?
    UNTIL DROP THEN ;

( Startup )

: VER ( -- u ) $101 ;
: hi ( -- )
    !IO BASE @ HEX \ initialize IO device & sign on
    CR ." eFORTH V" VER <# # # 46 HOLD # #> TYPE
    CR ;
: EMPTY ( -- )
    FORTH CONTEXT @ DUP CURRENT 2! 6 CP 3 MOVE OVERT ;
    CREATE 'BOOT ' hi , \ application vector
: COLD ( -- )
    BEGIN
    U0 UP 74 CMOVE
    PRESET 'BOOT @EXECUTE
    FORTH CONTEXT @ DUP CURRENT 2! OVERT
    QUIT
    AGAIN ;

