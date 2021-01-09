# The many faces of EForth

EForth is a delightfully minimalist approach to Forth originated by Bill Muench and Dr. C. H. Ting.

In its original form metacompilation is avoided.

## Directories

* reference/ - EForth Reference Material
* circleforth/ - A minimalist Forth modeled after toy Lisps.
* ueforth/ - micro EForth - EForth refactored to allow source boostraping.

## EForth Quirks

EForth uses `FOR..NEXT` in favor of `DO..LOOP`:
[FOR/NEXT](https://github.com/TG9541/stm8ef/wiki/eForth-FOR-..-NEXT).

This construct has the odd property that it iterates one "extra" time for zero.

```
: FOO 10 FOR R@ . NEXT ; FOO
 10 9 8 7 6 5 4 3 2 1 0  ok
```

To permit a more ordinary loop the `AFT` word is used in the sequence `FOR..AFT..THEN..NEXT`.

```
: FOO 10 FOR ( 1st time only ) AFT R@ . THEN NEXT ; FOO
 9 8 7 6 5 4 3 2 1 0  ok
```

The even more enigmatic `FOR..WHILE..NEXT..ELSE..THEN` is used in place of `DO..LEAVE..LOOP`.
It allows a while condition to early out of a counted loop.
