Title: XML Examples
Sort-Priority: 100

## XML Examples

Here's an example dialogue XML:

```xml
<DialogueTree>
	<NameField>EXAMPLE NPC</NameField>

	<DialogueNode>
		<Name>Start</Name>
		<EntryCondition>DEFAULT</EntryCondition>
		<Dialogue>
			<Page>Start</Page>
      		<Page>Start Part 2</Page>
		</Dialogue>
		
		<DialogueOptionsList>
			<DialogueOption>
				<Text>Goto 1</Text>
				<DialogueTarget>1</DialogueTarget>
			</DialogueOption>
			<DialogueOption>
				<Text>Goto 2</Text>
				<DialogueTarget>2</DialogueTarget>
			</DialogueOption>
			<DialogueOption>
				<Text>Goto End</Text>
				<DialogueTarget>End</DialogueTarget>
			</DialogueOption>
		</DialogueOptionsList>
	</DialogueNode>
	
	<DialogueNode>
		<Name>1</Name>
		<Dialogue>
			<Page>This is 1</Page>
		</Dialogue>
		
		<DialogueOptionsList>
			<DialogueOption>
				<Text>Goto 2</Text>
				<DialogueTarget>2</DialogueTarget>
			</DialogueOption>
			<DialogueOption>
				<Text>Goto End</Text>
				<DialogueTarget>End</DialogueTarget>
			</DialogueOption>
		</DialogueOptionsList>
	</DialogueNode>


	<DialogueNode>
		<Name>2</Name>
		<Dialogue>
			<Page>This is 2</Page>
		</Dialogue>

		<DialogueOptionsList>
			<DialogueOption>
				<Text>Goto 1</Text>
				<DialogueTarget>1</DialogueTarget>
			</DialogueOption>
			<DialogueOption>
				<Text>Goto End</Text>
				<DialogueTarget>End</DialogueTarget>
			</DialogueOption>
		</DialogueOptionsList>

	</DialogueNode>
	
	<DialogueNode>
		<Name>End</Name>
		<Dialogue>
			<Page>This is the end</Page>
		</Dialogue>
	</DialogueNode>

</DialogueTree>
```	  
