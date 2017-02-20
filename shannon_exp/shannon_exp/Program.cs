using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
 * Assumptions
 * 
 * support input with no and
 * or no or
 * inputs:
 * a + b
 * a
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
    class Program
    {
        class minterm
        {
            public List<char> data;
            public bool used;
            public minterm(minterm rhs)
            {
                data = rhs.data;
                used = rhs.used;
            }
            public minterm(string value)
            {
                data = value.ToList();
                used = false;
            }
            public override bool Equals(object obj)
            {
                return (string.Join("", data) == string.Join("", (obj as minterm).data));
            }
        }


        // input types
        // or > 5 variables
        // single variable
        //

        static void Main(string[] args)
        {
            string input = "~a & ~b & ~c | a & ~b & ~c | a & b & ~c | a & b & c";
            

            input = "~a & ~b & ~c | a & ~b & ~c | a & b & ~c | a & b & c";

            input = "a & b | ~b & c & d | a & c & d";



            List<string> binterms = new List<string>();
            HashSet<char> lit_hash = new HashSet<char>();

            /* Make hash set of all input characters */
            foreach (char ch in input)
                lit_hash.Add(ch);

            /* Remove operators and whitespace to leave variables left */
            foreach (char ch in " ~|&")
                lit_hash.Remove(ch);

            /* Make list of variables */
            List<char> lits = new List<char>();
            foreach (char ch in lit_hash)
                lits.Add(ch);


            System.Console.Write("Variables: ");
            foreach (char ch in lits)
                System.Console.Write("{0}", ch);
            System.Console.WriteLine();
             
            /* Split string by OR operators into minterms */
            List<string> minterms = input.Split('|').ToList();
            foreach (var minterm in minterms)
            {
                /* Make string containing all unused variables (e.g. abc -> xxx) */
                List<char> output = new String('x', lit_hash.Count()).ToList();

                /* Split minterm by AND operators into expressions */
                List<string> exprs = minterm.Split('&').ToList();

                /* Parse expressions */
                foreach (string expr in exprs)
                {
                    bool invert = false;
                    string temp = expr.Trim();

                    /* Scan expression to find 'var' or '~var' */
                    foreach (char ch in temp)
                    {
                        if (ch == '~')
                            invert = (invert) ? false : true; /* To handle '~a', '~~a', '~~~a', etc. */
                        else
                            if (lit_hash.Contains(ch))
                            {
                                /* Literal is assigned a true or false value, add that to output */
                                output[lits.IndexOf(ch)] = invert ? '0' : '1'; 
                                invert = false;
                            }
                            else
                            {
                                /* Unknown character found in expression (shouldn't happen) */
                                throw new NotImplementedException(String.Format("Parse error, found [{0}] in [{1}]", ch, temp));
                            }
                    }
                }

                /* Add binary minterm to list (unless it already exists) */
                string binterm = string.Join("", output);
                if (binterms.Contains(binterm))
                    throw new NotImplementedException(String.Format("Parse error, expression has redundant minterm [{0}]", binterm));
                binterms.Add(binterm);
            }

            /* Print all binary minterms */
            System.Console.Write("Binary minterms: ");
            foreach (var term in binterms)
                System.Console.Write("{0} ", term);
            System.Console.WriteLine();
 

            /* End of input string processing into binterms */

            //-----------------------------------------------------------------------------------------------------------------
            //-----------------------------------------------------------------------------------------------------------------
            
            /* Determine literal frequency to find factorization candidates */

            const int max_states = 2; /* 0,1 not x */
            List<int>[] term_frequency = new List<int>[max_states]; // var and ~var

            /* Make empty frequency list */
            for (int i = 0; i < max_states; i++)
                term_frequency[i] = new List<int>();
            for (int i = 0; i < lit_hash.Count; i++)
            {
                term_frequency[0].Add(0);
                term_frequency[1].Add(0);
            }

            /* Calculate term frequency */
            foreach (string binterm in binterms)
            {
                for (int i = 0; i < lit_hash.Count; i++)
                {
                    switch(binterm[i])
                    {
                    case '0':
                        term_frequency[0][i]++;
                        break;
                    case '1':
                        term_frequency[1][i]++;
                        break;
                    default:
                        break;
                    }
                }
            }


//             Print frequency of terms in minterms 
            for (int i = 0; i < max_states; i++)
            {
                System.Console.WriteLine("term {0}", i);
                foreach (int freq in term_frequency[i])
                    System.Console.WriteLine("{0}", freq);
            }
       
            /* Search for a term that occurs an equal of time in both true and false instances,
             * which will be the common control term.
             */
            int control = -1;
            for (int j = 0; j < lit_hash.Count; j++)
            {
                if (term_frequency[0][j] == term_frequency[1][j])
                {
                    control = j;
                    break;
                }
            }
            if (control != -1)
                System.Console.WriteLine("Term {0} ({1}) is candidate for control", control, lits[control]);
            else
            {
                System.Console.WriteLine("No control term found, cannot reduce.");
                return;
            }

            /* End of finding control term */

            //-----------------------------------------------------------------------------------------------------------------
            //-----------------------------------------------------------------------------------------------------------------

            /* Start of factoring out control term */

            List<string>[] partition = new List<string>[2];
            partition[0] = new List<string>();
            partition[1] = new List<string>();

            var minterm_ptn = new List<minterm>[2];
            minterm_ptn[0] = new List<minterm>();
            minterm_ptn[1] = new List<minterm>();

            foreach (string binterm in binterms)
            {
                /* Make new minterm with control term replaced with don't-care */
                StringBuilder builder = new StringBuilder(binterm);
                builder[control] = 'x';
                string modified = builder.ToString();

                /* Based on control signal, add minterm to either side of paritition */
                if(binterm[control] == '0')
                {
                    partition[0].Add(modified);
                    minterm_ptn[0].Add(new minterm(modified));
                }
                else
                if(binterm[control] == '1')
                {
                    partition[1].Add(modified);
                    minterm_ptn[1].Add(new minterm(modified));
                }
                else
                if(binterm[control] == 'x')
                {
                    /* Shouldn't happen */

                    // this does happen with ab+b'cd+acd
                    // this means each minterm can either use the minterm (true or complemented) or have it as a not-care
                    // verify allowing a don't-care for the control doesn't mess up later parts (minimization?)
                    // throw new NotImplementedException("Factoring error: Control term not used by a minterm.");
                }
            }

            /* Display unminimized partitions */
            for (int i = 0; i < 2; i++)
            {
                System.Console.WriteLine("Partition {0}", i);
                foreach (var x in partition[i])
                    System.Console.WriteLine("{0}", x);
            }

            var new_minterm = new List<minterm>[2];

            new_minterm[0] = new List<minterm>();
            new_minterm[1] = new List<minterm>();

            /* Minimize partitions */
            for (int i = 0; i < 2; i++)
            {
                foreach (minterm parent in minterm_ptn[i])
                {
                    foreach (minterm child in minterm_ptn[i])
                    {
                        int diff = 0;
                        int diff_pos = 0;

                        /* Don't compare the same minterm to itself */
                        if (parent == child)
                            continue;

                        /* Count number of differences between parent and child */
                        for (int index = 0; index < lit_hash.Count && diff <= 1; index++)
                        {
                            if (parent.data[index] != child.data[index])
                            {
                                diff++;
                                diff_pos = index;
                            }
                        }

                        /* If they differ by one bit, minimize */
                        if (diff == 1)
                        {
                            minterm derived = new minterm(parent);

                            /* Build minimized minterm */
                            derived.data[diff_pos] = 'x';
                            derived.used = false;

                            if (!new_minterm[i].Contains(derived))
                            {
                                parent.used = true;
                                child.used = true;

                                System.Console.WriteLine("Added {0}", string.Join("", derived.data));

                                new_minterm[i].Add(derived);

                                System.Console.WriteLine("Reduced P:{0} C:{1} D:{2} -> {3}",
                                        string.Join("", parent.data),
                                        string.Join("", child.data),
                                        diff,
                                        string.Join("", derived.data));
                            }
                        }
                        else
                        {
                            System.Console.WriteLine("P:{0} C:{1} D:{2}", parent, child, diff);
                        }
                    } // end foreach child
                } // end foreach parent

                /* Add minterms we couldn't minimize */
                foreach (minterm remaining in minterm_ptn[i])
                {
                    if (remaining.used == false && !new_minterm[i].Contains(remaining))
                        new_minterm[i].Add(remaining);
                }
            }

            /* Display unminimized partitions */
            for (int i = 0; i < 2; i++)
            {
                System.Console.WriteLine("Partition {0}", i);
                foreach (var x in new_minterm[i])
                    System.Console.WriteLine("{0}", string.Join("", x.data));
            }

            System.Console.WriteLine("--------------------------------");
            System.Console.WriteLine("Minimized minterms:");
            foreach (var minterm_list in new_minterm)
            {
                foreach (minterm m in minterm_list)
                {
                    System.Console.WriteLine("Minterm = [{0}]", String.Join("", m.data));
                }
            }


            System.Console.WriteLine("Control term: {0}", lits[control]);
            System.Console.Write("Expression: ");
            foreach (var minterm_list in new_minterm)
            {
                if (minterm_list == new_minterm[0])
                    System.Console.Write("~");

                System.Console.Write("{0}(", lits[control]);

                foreach (minterm m in minterm_list)
                {
                    string result = "";
                    bool wrote = false;

                    for(int i = 0; i < lit_hash.Count; i++)
                    {
                        if (m.data[i] != 'x')
                        {
                            if (m.data[i] == '0')
                                result += '~';
                            result += lits[i];
                            wrote = true;
                        }
                    }

                    if (wrote)
                    {
                        wrote = false;
                        System.Console.Write("{0}", result);
                        if(m != minterm_list.Last())
                            System.Console.Write(" | ");
                    }
                }
                System.Console.Write(")");

                if (minterm_list != new_minterm.Last())
                    System.Console.Write(" + ");
            }


            System.Console.ReadKey();

        }
    }
}


/* End */




