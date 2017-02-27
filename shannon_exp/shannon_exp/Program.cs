using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if false
    API
  
    string result;

 
    ShannonExpansion foo = new ShannonExpansion()
    foo.expand("a + b", ref result);
    
    ShannonExpansion foo = new ShannonExpansion("a + b");
    result = foo.expand();
    
 
#endif

/*
 * Some observations:
 * - Minimizing reduces the number of minterms associated with a function. since a CFGLUT5 has 
 *   enough storage for all possible minterms, minimization isn't necessary.
 *   (However it does make the results of the program more human-readable)
 * 
 * 
 * Minimizing input that only uses OR:
 * 
 * 
 * 
 * Assumptions
 * 
 * support input with no and
 * or no or
 * inputs:
 * a + b
 * a
 * needs to handle pos input (currently sop only)
 * sop: f(a,b,c,d) = ab+b'cd + acd
 * pos: f(a,b,c,d) = (a+b+c)(a'+d)(b+c+d)
 * 
 * Shannon's expansion can be applied to a function to determine a common control term such that the function can be represented by a multiplexer,
 * with the control term as the select and the remaining minterms as the inputs.
 * 
 * Therefore if we identify a common control term we can partition the function's minterms into two groups (one with the control term being true,
 * the other being negated) and reduce those groups to remove variables that no longer needed now that the control term is removed.
 * 
 * Input:
 *  Handles missing and present variables (can be 0, 1, X).
 *  Handles minterms listing variables out of order (~a&b&c vs c&~a&b evaluate the same).
 *  Handles multiple negation (~~a, etc.)
 *  Aborts on redundant minterms (not supported, though user shouldn't provide such input?).
 *  Assumes input variables are single characters (e.g. a-z or A-Z)
 */
namespace shannon_exp
{
    public class Program
    {
    

        static void Main(string[] args)
        {
            string input = "~a & ~b & ~c | a & ~b & ~c | a & b & ~c | a & b & c";

            //            input = "~a & ~b & ~c | a & ~b & ~c | a & b & ~c | a & b & c";
            //          input = "a & b | ~b & c & d | a & c & d";
            //            input = "a | b | c & ~a | d & b | c | d";
            //        input = "~a & b | ~a & c | a & d";

//            input = "~x & ~y & ~z | ~x & ~y & z | ~x & y & z | x & ~y & ~z + x & ~y & z";

          //  input = "x'y'z'+x'y'z+x'yz+xy'z'+xy'z";

            input = "d4 = ~d0 & ~d1 & ~d2 | d0 & ~d1 & ~d2 | d0 & d1 & ~d2 | d0 & d1 & d2 | d3";
        //  input = "f=~a & ~b & ~c | a & ~b & ~c | a & b & ~c | a & b & c | d";

            

            ShannonExpansion s = new ShannonExpansion();
            string output = "";
            s.expand(input, ref output);
            System.Console.WriteLine(output);
            System.Console.ReadKey();
        }
    }
}


/* End */