from collections import Counter
from enum import Enum

expr = 'f=~a * ~b & ~c | a & ~b * c + a & b & ~c | a & b & c | d'
ops = {'*': '&', '\'': '~', '+': '|'}          # add operators to dictionary


class TokenType(Enum):
    operator = 0
    variable = 1


class Token:

    def __init__(self, word, token_type):
        self.word = word
        self.token_type = token_type
        return


class SubExpression:

    def __init__(self, tokens):
        self.tokens = tokens
        lit_hash = set()    # might not need set if we are iterating only
        for token in tokens:
            if token.token_type == TokenType.variable:
                lit_hash.add(token.word)

        self.lits = lit_hash
        return

    def to_string(self):
        temp = ""
        for token in self.tokens:
            temp += token.word
        return temp


class Expression:
    def __init__(self, input_expr):
        self.lhs = SubExpression([])
        self.rhs = SubExpression([])
        self.expression = ""
        self.parse_string(input_expr)
        return

    def sanitize_input(self, expression):
        self.expression = expression.replace('*', '&')
        self.expression = self.expression.replace('\'', '~')
        self.expression = self.expression.replace('+', '|')
        return self.expression

    def parse_string(self, input_expr):
        tokens = []
        op_hash = {'|', '&', '~', '='}
        literal = ""

        input = self.sanitize_input(input_expr)
        for ch in input:
            if ch in op_hash:
                if literal != "":
                    tokens.append(Token(literal, TokenType.variable))
                    literal = ""
                tokens.append(Token("".join(ch), TokenType.operator))
            else:
                literal += ch

        if literal != "":
            tokens.append(Token(literal, TokenType.variable))

        if '\'' in input:
            index = 0
            for token in tokens:
                if token.token_type == TokenType.operator and token.word == "\'":
                    tokens.remove(token)
                    tokens.insert(index, Token('~', TokenType.operator))
                index += 1

        self.lhs = SubExpression(tokens[0:1])
        self.rhs = SubExpression(tokens[2:len(tokens)])

        return


class Minterm:
    def __init__(self, value=None, position=None, replacement=None):
        if value is not None:
            self.data = value.data
            self.used = value.used
        else:
            self.data = value
            self.used = False

        if position is not None:
            self.data[position] = replacement
            self.used = False

        return

    def __eq__(self, other):
        lhs = "".join(self.data)
        rhs = "".join(other.data)
        return lhs == rhs


class ShannonExpansion:
    def __init__(self, input_expr):
        self.input_expr = input_expr
        return

    @staticmethod
    def make_minterms_from_expression(expression):
        invert = False
        minterm = Minterm()
        minterms = []

        for token in expression.rhs.tokens:
            if minterm is None:
                minterm = Minterm('x' * len(expression.rhs.lits))

            if token.token_type == TokenType.variable:
                minterm.data[expression.rhs.lits.index(token.word)] = '0' if invert else '1'
                invert = False
            elif token.token_type == TokenType.operator:
                if token.word == '|':
                    minterms.append(minterm)
                    invert = False
                    minterm = None
                elif token.word == '~':
                    invert = ~invert
                else:
                    invert = False

        if minterm is not None:
            minterms.append(minterm)

        return minterms


class ControlData:

    def __init__(self):
        self.term_frequency = []
        self.control_frequency = 0
        self.control = 0
        return

    def find_variable_frequency(self, minterms, max_variables):
        MAX_STATES = 2

        for i in range(MAX_STATES):
            self.term_frequency.append([])

        for i in range(MAX_STATES):
            for j in range(max_variables):
                self.term_frequency[i].append(0)

        for minterm in minterms:
            for i in range(MAX_STATES):
                if minterm.data[i] == '0':
                    self.term_frequency[0][i] += 1
                else:
                    if minterm.data[i] == '1':
                        self.term_frequency[1][i] += 1
        return

    def find_control_variable(self):
        control = -1
        control_freuency = 0

        for j in range(len(self.term_frequency[0])):
            currentMin = min(self.term_frequency[0][j], self.term_frequency[1][j])
            if currentMin > control_freuency:
                control = j
                self.control_frequency = currentMin
        return control != -1

    def expand(self, input_expr, result):
        expr = Expression(input_expr)
        minterms = ShannonExpansion.make_minterms_from_expression(expr)

        ctrlData = ControlData()
        ctrlData.find_variable_frequency(minterms, len(expr.rhs.lits))

        return


class Cfg:
    expression = ""
    terms = ""

    def __cleanup_expression(self):

        # remove operators
        self.terms = self.expression.replace('&', '')
        self.terms = self.terms.replace('~', '')
        self.terms = self.terms.replace('|', '')

        print("expression is " + self.expression)
        print("terms are: " + self.terms)
        return

    @staticmethod
    def __generate_minterms(self):

        return

    # this is the actual function that will be exposed
    def bool_func(self, bool_expr):

        # remove lhs to work with only what we care about
        temp_list = bool_expr.split("=")
        self.expression = temp_list[1]
        self.expression = "".join(self.expression.split())  # remove whitespaces

        self.__cleanup_expression()

        c = Counter()

        return

test = Cfg()
Cfg.bool_func(test, expr)
