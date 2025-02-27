       IDENTIFICATION DIVISION.                                         
       PROGRAM-ID. DISPATCHER.                                          

       DATA DIVISION.
       WORKING-STORAGE SECTION.
       
       01  WS-PROGRAM-NAME          PIC X(30)      VALUE SPACES.
       01  WS-METHOD-NAME           PIC X(30)      VALUE SPACES.
       01  WS-ARG-COUNT             PIC 99         VALUE 0.
       01  WS-STATUS                PIC 9          VALUE 0.

       01  WS-ARGS.
           05  WS-ARG-VALUE         PIC X(100)     OCCURS 10 TIMES
                                              INDEXED BY IDX.

       01  PROG-IDX                 PIC 99         VALUE 0.
       01  METH-IDX                 PIC 99         VALUE 0.

       01  PROGRAM-TABLE.
           05  PROGRAM-ENTRY        OCCURS 5 TIMES.
               10  PROGRAM-NAME     PIC X(30)      VALUE SPACES.
               10  METHOD-TABLE     OCCURS 5 TIMES.
                   15  METHOD-NAME  PIC X(30)      VALUE SPACES.
                   15  METHOD-REF   PIC X(30)      VALUE SPACES.

       PROCEDURE DIVISION.
       MAIN-PROGRAM.
           DISPLAY "Enter program name: "
           ACCEPT WS-PROGRAM-NAME.

           DISPLAY "Enter method name: "
           ACCEPT WS-METHOD-NAME.

           DISPLAY "Enter argument count: "
           ACCEPT WS-ARG-COUNT.

           IF WS-ARG-COUNT > 0 THEN
              PERFORM VARYING IDX FROM 1 BY 1 
              UNTIL IDX > WS-ARG-COUNT
                 DISPLAY "Enter argument " IDX ": "
                 ACCEPT WS-ARG-VALUE(IDX)
              END-PERFORM
           END-IF.

           PERFORM INIT-PROGRAM-TABLE.
           PERFORM RESOLVE-METHOD.

           IF WS-STATUS = 0 THEN
              PERFORM DYNAMIC-CALL
           ELSE
              DISPLAY "Error: Method not found."
           END-IF.

           STOP RUN.

       INIT-PROGRAM-TABLE.
           MOVE SPACES TO PROGRAM-TABLE.

           MOVE "MYPROGRAM" TO PROGRAM-NAME(1).
           MOVE "MYFUNCTION" TO METHOD-NAME(1, 1).
           MOVE "MYFUNCTION-CALL" TO METHOD-REF(1, 1).

           EXIT.

       RESOLVE-METHOD.
           MOVE 0 TO WS-STATUS.

           PERFORM VARYING PROG-IDX FROM 1 BY 1 
           UNTIL PROG-IDX > 5
              IF PROGRAM-NAME(PROG-IDX) = WS-PROGRAM-NAME THEN
                 PERFORM VARYING METH-IDX FROM 1 BY 1 
                 UNTIL METH-IDX > 5
                    IF METHOD-NAME(PROG-IDX, METH-IDX) = 
                       WS-METHOD-NAME THEN
                       MOVE 0 TO WS-STATUS
                       EXIT PERFORM
                    END-IF
                 END-PERFORM
              END-IF
           END-PERFORM.

           IF WS-STATUS NOT = 0 THEN
              DISPLAY "Method " WS-METHOD-NAME " not found in program "
                      WS-PROGRAM-NAME
           END-IF.

           EXIT.

       DYNAMIC-CALL.
           EVALUATE WS-METHOD-NAME
               WHEN "MYFUNCTION"
                   PERFORM MYFUNCTION-CALL
               WHEN OTHER
                   DISPLAY "Unknown method: " WS-METHOD-NAME
                   MOVE 1 TO WS-STATUS
           END-EVALUATE.

           EXIT.

       MYFUNCTION-CALL.
           DISPLAY "Dynamic call: MYFUNCTION with arguments: ".
           PERFORM VARYING IDX FROM 1 BY 1 
           UNTIL IDX > WS-ARG-COUNT
              DISPLAY "Argument " IDX ": " WS-ARG-VALUE(IDX)
           END-PERFORM.

           EXIT.
