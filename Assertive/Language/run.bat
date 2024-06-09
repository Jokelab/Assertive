call antlr AssertiveLexer.g4
call antlr AssertiveParser.g4

call javac Assertive*.java
call grun Assertive program google.ass -gui
