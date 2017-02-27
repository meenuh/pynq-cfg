using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shannon_exp
{
    /* A token can be an operator or a variable */
    public enum TokenType
    {
        Operator,
        Variable
    };

    /* An operator can be any of the following operations */
    public enum OperatorType
    {
        LogicAnd,
        LogicOr,
        LogicNot,
        LogicXor,
    };

    /* A single variable or operator in an expression */
    public class Token
    {
        public string word;
        public TokenType type;
        public OperatorType op;

        public Token(TokenType type)
        {
            this.word = "";
            this.type = type;
        }

        public Token(string word, TokenType type)
        {
            this.word = word;
            this.type = type;
        }
    };

    public class Expression
    {
        /* The left and right sides of the expression */
        public SubExpression lhs, rhs;

        /* Build a string containing the LHS, assignment operator, and RHS */
        public override string ToString()
        {
            return lhs.ToString() + "=" + rhs.ToString();
        }

        /* Trim input and standardize operators used */
        private string sanitizeInput(string input)
        {
            var output = new List<char>();

            /* Convert expression to use standard operators and trim white space */
            foreach (char src in input)
            {
                switch (src)
                {
                case ' ':   /* Remove all whitespace from input */
                case '\t':
                case '\v':
                break;

                case '+':   /* Replace alternative operator with standard one */
                output.Add('|');
                break;

                case '*':   /* Replace alternative operator with standard one */
                output.Add('&');
                break;

                case '!':   /* Replace alternative operator with standard one */
                output.Add('~');
                break;

                default:
                output.Add(src);
                break;
                }
            }

            /* Return list of characters as a string */
            return String.Join("", output.ToArray());
        }

        /* Parse a string into tokens */
        public void parseString(string input)
        {
            var op_hash = new HashSet<char>("'|&~=");
            var tokens = new List<Token>();

            /* Convert input to standard form */
            input = sanitizeInput(input);

            /* Parse sanitized input and split into variables and operators */
            string literal = "";
            foreach (char ch in input)
            {
                /* Found an operator */
                if (op_hash.Contains(ch))
                {
                    /* If we had been building up a literal, add it as a new token */
                    if (literal != "")
                    {
                        tokens.Add(new Token(literal, TokenType.Variable));
                        literal = "";
                    }

                    /* Add operator as new token */
                    tokens.Add(new Token(String.Join("", ch), TokenType.Operator));
                }
                else
                {
                    /* Found part of a literal */
                    literal += ch;
                }
            }

            /* If there is a literal remaining, add it */
            if (literal != "")
                tokens.Add(new Token(literal, TokenType.Variable));

            /* If postfix negation (') is present, convert to prefix form (~) */
            if (input.Contains('\''))
            {
                for (int index = 0; index < tokens.Count; index++)
                {
                    if (tokens[index].type == TokenType.Operator && tokens[index].word == "'")
                    {
                        /* Remove back-tick */
                        tokens.RemoveAt(index);

                        /* Insert negation operator before token */
                        tokens.Insert(index - 1, new Token("~", TokenType.Operator));
                    }
                }
            }

            /* Split expression into a left subexpression (LHS) and right subexpression (RHS) */
            lhs = new SubExpression(tokens.GetRange(0, 1));
            rhs = new SubExpression(tokens.GetRange(2, tokens.Count - 2));
        }
    }

    public class SubExpression
    {
        /* Sequence of tokens that define the expression */
        public List<Token> tokens;

        /* Hash set of all unique variables */
        public HashSet<String> lit_hash;

        /* List of all unique variables */
        public List<String> lits;

        /* Build a string containing the tokens in this subexpression */
        public override string ToString()
        {
            string temp = "";
            foreach (var token in tokens)
                temp += token.word;
            return temp;
        }

        /* Build subexpression based on token list */
        public SubExpression(List<Token> tokens)
        {
            /* Assign tokens */
            this.tokens = new List<Token>(tokens);

            /* Make hash of unique literals */
            lit_hash = new HashSet<string>();
            foreach (var token in tokens)
                if (token.type == TokenType.Variable)
                    lit_hash.Add(token.word);

            /* Make list of variables */
            lits = new List<String>(lit_hash.ToArray());
        }
    };



    /* Helper class to represent a minterm */
    public class minterm
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

    public class ShannonExpansion
    {
        public string input;

        public ShannonExpansion()
        {
        }

        public ShannonExpansion(string input)
        {
            this.input = input;
        }

        public string getLastError()
        {

            return null;
        }

        /* Make binary minterms */
        public List<string> makeBinterms(Expression expr)
        {
            List<string> binterms = new List<string>();
            List<char> binterm = null;
            bool invert = false;

            foreach (var token in expr.rhs.tokens)
            {
                /* Make a character list with as many x's as there are unique variables in the RHS of the expression (e.g. "f=abc+ac" produces "xxx") */
                if (binterm == null)
                    binterm = new List<char>(new String('x', expr.rhs.lit_hash.Count()).ToArray());

                switch (token.type)
                {
                    case TokenType.Variable:
                        binterm[expr.rhs.lits.IndexOf(token.word)] = (invert) ? '0' : '1';
                        invert = false;
                        break;

                    case TokenType.Operator:
                        switch (token.word)
                        {
                            case "|": /* Split sub-expression by OR operator into minterms */
                                binterms.Add(String.Join("", binterm.ToArray()));
                                invert = false;
                                binterm = null;
                                break;

                            case "~":
                                invert = !invert;
                                break;

                            default:
                                invert = false;
                                break;
                        }
                        break;
                }
            }
            if (binterm != null)
                binterms.Add(String.Join("", binterm.ToArray()));

            return binterms;
        }

        public bool expand(string input, ref string result)
        {
            result = null;

            Expression expr = new Expression();
            expr.parseString(input);
            var binterms = makeBinterms(expr);

// DEBUG
            /* Print expression */
            System.Console.WriteLine(expr);

            /* Print all variables used */
            System.Console.Write("LHS Variables: ");
            foreach (var name in expr.lhs.lits)
                System.Console.Write("{0}", name);
            System.Console.WriteLine();

            /* Print all variables used */
            System.Console.Write("RHS Variables: ");
            foreach (var name in expr.rhs.lits)
                System.Console.Write("{0}", name);
            System.Console.WriteLine();

            /* DEBUG: Print finalized expression */
            System.Console.Write("LHS Tokens: ");
            foreach (var token in expr.lhs.tokens)
                System.Console.Write("{0}", token.word);
            System.Console.WriteLine();

            /* DEBUG: Print finalized expression */
            System.Console.Write("RHS Tokens: ");
            foreach (var token in expr.rhs.tokens)
                System.Console.Write("{0}", token.word);
            System.Console.WriteLine();

            System.Console.WriteLine("Binterms:");
            foreach (var term in binterms)
                System.Console.WriteLine("{0}", term);
// END_DEBUG
            /* End of input string processing into binterms */

            //-----------------------------------------------------------------------------------------------------------------
            //-----------------------------------------------------------------------------------------------------------------

            /* Determine literal frequency to find factorization candidates */

            int max_lits = expr.rhs.lit_hash.Count();
            const int max_states = 2; /* 0,1 not x */
            List<int>[] term_frequency = new List<int>[max_states];

            /* Make empty frequency list */
            for (int i = 0; i < max_states; i++)
                term_frequency[i] = new List<int>();

            for (int i = 0; i < max_lits; i++)
            {
                term_frequency[0].Add(0);
                term_frequency[1].Add(0);
            }

            /* Calculate term frequency */
            foreach (string binterm2 in binterms)
            {
                for (int i = 0; i < max_lits; i++)
                {
                    switch (binterm2[i])
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


            // Print frequency of terms in minterms 
            for (int i = 0; i < max_states; i++)
            {
                System.Console.Write("Term frequency {0}: ", i);
                foreach (int freq in term_frequency[i])
                    System.Console.Write("{0} ", freq);
                System.Console.WriteLine();
            }

            /* Search for a term that occurs an equal of time in both true and false instances,
             * which will be the common control term.
             */
            int control = -1;
            for (int j = 0; j < max_lits; j++)
            {
                if (term_frequency[0][j] == term_frequency[1][j])
                {
                    control = j;
                    break;
                }
            }

            if (control == -1)
            {
                // for input = "~a & b | ~a & c | a & d";
                // 2000
                // 1111
                // hw1_sol.pdf says A is the term to pull out
                // so equal or more coverage?
                // how to pick to not conflict
                // a has at least 1:1 coverage plus more

                // for input = "~a & ~b & ~c | a & ~b & ~c | a & b & ~c | a & b & c";
                // 321
                // 123
                // b has 2:2 coverage so we pick it
                // total coverage is 4,4,4, so we pick b which has equal coverage
                control = 0;

            }

            if (control != -1)
                System.Console.WriteLine("Term {0} ({1}) is candidate for control", control, expr.rhs.lits[control]);
            else
            {
                System.Console.WriteLine("No control term found, cannot reduce.");
                System.Console.ReadKey();
                return true;
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
                if (binterm[control] == '0')
                {
                    partition[0].Add(modified);
                    minterm_ptn[0].Add(new minterm(modified));
                }
                else
                    if (binterm[control] == '1')
                    {
                        partition[1].Add(modified);
                        minterm_ptn[1].Add(new minterm(modified));
                    }
                    else
                        if (binterm[control] == 'x')
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
                        for (int index = 0; index < max_lits && diff <= 1; index++)
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

            System.Console.WriteLine("Control term: {0}", expr.rhs.lits[control]);
            System.Console.Write("Expression: ");
            foreach (var minterm_list in new_minterm)
            {
                if (minterm_list == new_minterm[0])
                    System.Console.Write("~");

                System.Console.Write("{0}(", expr.rhs.lits[control]);

                foreach (minterm m in minterm_list)
                {
                    string result2 = "";
                    bool wrote = false;

                    for (int i = 0; i < max_lits; i++)
                    {
                        if (m.data[i] != 'x')
                        {
                            if (m.data[i] == '0')
                                result2 += '~';
                            result2 += expr.rhs.lits[i];
                            wrote = true;
                        }
                    }

                    if (wrote)
                    {
                        wrote = false;
                        System.Console.Write("{0}", result2);
                        if (m != minterm_list.Last())
                            System.Console.Write(" | ");
                    }
                }
                System.Console.Write(")");

                if (minterm_list != new_minterm.Last())
                    System.Console.Write(" + ");
            }
//#endif
            return false;
        }
        public string expand()
        {
            string result = "";
            expand(input, ref result);
            return result;
        }
    }

}
