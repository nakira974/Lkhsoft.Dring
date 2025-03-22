//
// Created by maxim on 22/03/2025.
//

#ifndef EXCEPTION_H
#define EXCEPTION_H
#include <setjmp.h>

/* ######### ERROR HANDLING MACROS  #########*/

/* Try instruction  */
#define TRY do { if (setjmp(error_jmp_buf) == 0) {
/* Catch instruction */
#define CATCH } else {
/* Finally instruction */
#define FINALLY } } while (0);
/* Throw instruction */
#define THROW longjmp(error_jmp_buf, 1)

#endif //EXCEPTION_H
