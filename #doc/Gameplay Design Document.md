# Gameplay Design Document

## Design Intent

- **Core Experience and Player Goal**

    - Players act as a zombie matchmaker, using observation, questioning, and evidence to find someone who genuinely survived the injury requested by a client\.

    - The main appeal is finding contradictions, exposing deception, and gradually uncovering what happened\.

- **Target Audience and Level Length**

    - Designed for players who enjoy story\-driven mysteries, character interaction, and dark humor\.

    - Each level aims to last 15–20 minutes, subject to playtesting\.

- **Investigation, Deduction, and Evidence**

    - Players discover clues through bodies, personal belongings, and conversations\.

    - Presenting evidence against specific statements leads to revised accounts and new discoveries\.

    - Progress depends on connecting information, rather than clicking everything or repeatedly guessing\.

- **Dark Humor and Zombie Romance**

    - Injuries that humans find horrifying become attractive qualities in zombie society\.

    - Humor comes from unusual accidents, body repairs, and character reactions\.

- **Character Development and Story Twists**

    - Candidates have different motives and secrets\.

    - Lying does not necessarily mean an injury story is false\. A genuine survivor may exaggerate how heroic they were\.

- **Fair Information and Failure Feedback**

    - Injuries, repairs, and disguises follow consistent rules\.

    - Important conclusions are supported by discoverable evidence\.

    - Free investigation is clearly separated from risky formal challenges\.

    - Incorrect deductions receive useful feedback, and failures allow checkpoint retries\.

- **Project Scope: Story\-Driven Levels Without Progression Systems**

    - The game progresses through individual story cases\.

    - There are no action points, real\-time countdowns, facility upgrades, or character stat growth\.

    - Investigation and evidence challenges form the main gameplay\. Client reactions and date scenes provide narrative feedback\.

## Game Background

### World Setting

- **Zombie Society After the Apocalypse**

    - After the zombie war, zombies gradually regain their thoughts and emotions, rebuild society, and begin seeking romance\.

    - Decay, missing limbs, and body repairs are normal parts of daily life\.

- **Zombie Resilience and Body Repairs**

    - Zombies can survive severe and absurd accidents\.

    - Damaged body parts can be repaired or replaced, so their current appearance may differ from their condition after an accident\.

- **Unusual Injuries as Romantic Appeal**

    - Extraordinary injury stories are considered attractive and worth sharing\. Scars become something like personal medals\.

    - Some zombies admire someone who was burned to charcoal\. Others believe that only someone struck by lightning understands a “spark\.”

- **Consistent Rules for Injuries, Disguises, and Evidence**

    - Appearance can be faked, but the physical effects of an accident must remain consistent\.

    - Makeup changes appearance but cannot directly reproduce deep damage\.

    - Repairs and replacements leave joining marks and may remove earlier injuries\.

    - Physical clues can support a type of injury, but cannot independently prove when, where, or why it happened\.

    - Players must combine physical clues, objects, records, and statements\.

    - Important rules are introduced before they are needed for a deduction\.

- **Zombie Celebrities and Public Attention**

    - Celebrity relationships attract public interest, and their dating preferences are widely promoted\.

    - Unusual injury stories become valuable selling points for potential partners\.

- **Celebrity Romance and Impostors**

    - Some admirers fake injuries, invent accidents, or claim someone else’s experiences\.

    - Matchmaking agencies must verify the truth behind these romantic stories\.

### Main Story

- **The Protagonist and Their Celebrity Aunt**

    - The protagonist’s aunt is a well\-connected zombie socialite and the agency’s main source of clients\.

- **The Agency’s Origin and Business**

    - With their aunt’s support, the protagonist opens a matchmaking agency specializing in celebrity referrals\.

- **Unusual Celebrity Requests**

    - Clients seek partners who survived specific, bizarre injuries because they find those experiences romantically appealing\.

- **Advance Contact and Incomplete Candidate Information**

    - The protagonist contacts potential candidates through introduction letters, voice messages, and similar sources\.

    - The available information is limited, and their stories have not been independently verified\.

- **Publicity and the Arrival of Impostors**

    - Public announcements attract admirers who fake injuries and memorize the advertised story\.

    - Several candidates arrive, each claiming to meet the client’s request\.

- **The Protagonist’s Task: Find the Genuine Survivor**

    - Players investigate clues and statements to reconstruct the accident\.

    - The goal is to verify the injury experience, rather than simply identify the person originally contacted\.

## Gameplay Flow

- **Introduction and Worldbuilding**

    - A conversation with the aunt introduces zombie romance, the agency’s origin, and the player’s role\.

- **Receive a Referral**

    - The aunt introduces the celebrity client and the background of the request\.

- **Discuss the Client’s Requirements**

    - The client explains the injury experience they seek and why they find it attractive\.

- **Review Appointment Information and Publicity**

    - The commission file opens when the request is first received\.

    - It then moves into a secondary menu that players can reopen at any time\.

- **Meet Multiple Candidates**

    - Each candidate gives their version of the accident and claims to qualify\.

- **Switch Between Candidates**

    - Players choose their investigation order and revisit characters freely\.

- **Observe Characters and Objects**

    - Click body parts, clothing, and belongings to discover clues at no cost\.

- **Talk, Question, and Collect Evidence**

    - Gather accident details, important statements, and usable evidence\.

    - Completed conversations can be replayed for free\.

- **Select Statements and Present Evidence**

    - Collecting the necessary evidence unlocks “Begin Cross\-Examination\.”

    - During this phase, players challenge specific statements with evidence\.

- **Expose Contradictions and Unlock Dialogue**

    - Players can repeatedly review the testimony until they identify the contradiction\.

    - A successful challenge ends the round and opens new dialogue or investigation\.

- **Verify the Injury Experience**

    - Combine evidence to distinguish fabricated experiences, mistaken memories, and hidden motives\.

- **Confirm a Qualified Candidate**

    - Submit a final selection once the evidence is sufficient\.

    - Fully exposing every impostor is optional\.

- **Explain the Truth to the Client**

    - Describe the actual injury and any exaggerations or hidden details\.

- **The Client Decides Whether to Meet**

    - The client makes an informed decision through dialogue\.

- **Date Scene and Case Epilogue**

    - Show the date or rejection aftermath and its effect on the characters\.

- **Move to the Next Case**

    - The aunt introduces another request, accident, and set of deceptions\.

- **Incorrect Challenges, Failure, and Retry**

    - Incorrect formal challenges reduce patience\.

    - When patience runs out, a candidate bites the protagonist, triggering a checkpoint retry\.

## Game Design

### Core Gameplay Systems

The following examples use a client seeking **a zombie who was burned to charcoal in a fire and survived**\.

- **Client Requests and Investigation Goals**

    - Each case focuses on a specific injury experience\.

    - The commission file contains the client’s request, appointment information, and public announcements\.

    - Order details are not permanently displayed\. Players open the desk’s commission file to view them in a secondary menu\.

    - Viewing the file does not reduce patience or advance the story\. Closing it returns players to their previous position\.

    - Public information alone cannot prove that someone experienced the accident\.

    - For example, all three candidates know the restaurant’s name because it appeared in the announcement\.

- **Candidate Selection and Reception**

    - Players freely choose whom to investigate without losing progress\.

    - Information from one candidate can be used when questioning another\.

    - New evidence may unlock topics for other characters\.

    - For example, one candidate is completely blackened, another knows the restaurant well, and a third has extensive repairs\. None of these traits alone proves who suffered the injury\.

- **Character Hotspots and Scene Observation**

    - Body parts, clothing, belongings, and selected scene objects are clickable\.

    - Hovering displays a label; clicking opens an observation or conversation\.

    - Hotspots distinguish between unseen content, new information, and completed interactions\.

    - Observations present facts without automatically declaring someone a liar\.

    - For example, black paint between someone’s fingers proves that their appearance was altered, but not that they never suffered burns\.

- **Abstract Topic Bubbles**

    - Questions without a clear physical object appear as bubbles beside the character\.

    - Topics use specific questions, such as “Where were you when the fire started?”

    - Observation, dialogue, and evidence unlock new topics\.

    - Completed topics remain available for replay, while new ones receive a brief highlight\.

- **Layered Dialogue and Follow\-Up Questions**

    - Initial account: the candidate’s voluntary version of events\.

    - Detailed questions: locations, actions, injuries, and events after the accident\.

    - Revised explanation: information revealed after a successful challenge\.

    - Ordinary questions are free and do not reduce patience\.

    - For example, the genuine survivor may admit that they stayed inside for a prepaid brain\-meat meal, rather than to rescue someone\.

- **Dialogue Replay**

    - Clicking a completed hotspot or topic replays its conversation\.

    - Replay costs no patience and does not award duplicate evidence\.

    - When an interaction also has new content, players choose between replaying the conversation and asking further questions\.

    - Important statements retain their original wording\.

    - No separate interview notebook or case board is included at this stage\.

- **Evidence Collection and Inspection**

    - Evidence comes from observations, conversations, appointment information, and accident\-related objects\.

    - Each item includes a name, image, confirmed facts, and source\.

    - Descriptions do not directly identify which statement to challenge\.

    - New discoveries may update descriptions, but cannot silently change established facts\.

    - Examples include:

        - Black paint: proves that the body was painted\.

        - Evacuation footage: establishes someone’s location at a particular time\.

        - Repair records: document treatment, but still require a verified connection to the candidate\.

- **Statement Selection**

    - Evidence challenges are limited to clearly defined cross\-examination phases\.

    - “Begin Cross\-Examination” becomes available once the necessary evidence has been collected\.

    - The protagonist asks the candidate to repeat a key part of their account, and the interface announces the phase\.

    - Each round contains approximately 3–5 statements, displayed one at a time\.

    - Players move backward or forward and can question or challenge each statement\.

    - There are no timed inputs, and the contradictory statement is not highlighted in advance\.

- **Evidence Presentation and Contradiction Checks**

    - The sequence is: select a statement → select evidence → confirm the challenge\.

    - Browsing, changing selections, and canceling carry no penalty\.

    - A successful challenge must establish a specific logical contradiction\.

    - Multiple reasonable evidence solutions should be accepted where appropriate\.

    - An incorrect challenge reduces patience and returns players to the current statement\.

    - At the end of unresolved testimony, “Review Testimony Again” returns to the first statement\.

    - Players advance the loop manually; it does not autoplay\.

    - For example, paint contradicts “I have never put anything on my skin,” but does not contradict “I entered the restaurant\.”

- **New Clues and Dialogue Unlocks**

    - Successful evidence presentation immediately stops the testimony loop\.

    - The candidate may admit part of their account was false, provide new details, or reveal a previously hidden object\.

    - “Cross\-Examination Complete” clearly marks the end of the round\.

    - Each round focuses on one main contradiction\.

    - Deeper lies require a new investigation and another round\.

    - For example, admitting to using paint leads to further investigation of whether genuine burn damage exists underneath\.

- **Injury Verification and Final Identification**

    - Players determine whether the experience is real, not whether the candidate has always been truthful\.

    - Conclusions combine the accident timeline, physical damage, repair history, and other testimony\.

    - Collecting key evidence unlocks final identification\.

    - Players choose a candidate and the evidence supporting their conclusion\.

    - A correct identification leads to approval and the client briefing\.

    - Incorrect identification follows the formal challenge penalty rules\.

    - For example, remaining charred tissue, verified repair records, and witness testimony may establish the injury even if the survivor’s motive was embarrassing\.

- **Optional Impostor Exposure**

    - Once the correct candidate is verified, players may finish the main investigation\.

    - Fully exposing other impostors is optional\.

    - Additional investigation reveals methods, motives, or epilogue variations\.

    - It provides no progression rewards and is not a hidden completion requirement\.

### Level and Narrative Systems

- **Story\-Driven Level Progression**

    - Each case has its own client, accident, and conclusion\.

    - The aunt connects cases through opening and closing conversations\.

    - Progress comes from investigation, cross\-examination, and narrative outcomes rather than resource accumulation\.

- **Tutorials and Difficulty Progression**

    - The first case introduces observation, questioning, evidence inspection, and formal challenges\.

    - The first cross\-examination contains one direct contradiction\.

    - Later cases introduce conflicting accounts, hidden motives, mistaken memories, and complicated repairs\.

    - Difficulty comes from evidence relationships rather than excessive irrelevant information or unclear wording\.

    - New rules are taught before they become necessary\.

- **Public Information, Private Information, and Leaks**

    - The game distinguishes public announcements, unverified appointment information, and confirmed investigation facts\.

    - Knowing private details does not automatically prove firsthand experience; their source must also be considered\.

    - Information leaks must leave discoverable clues\.

    - For example, someone may know the restaurant layout because they worked there, not because they were badly burned there\.

- **Truth, Mistaken Memories, Concealment, and Lies**

    - Contradictions may result from faulty memory, hidden motives, or deliberate fabrication\.

    - Evidence first establishes a problem with a statement; further investigation explains why\.

    - One successful challenge does not automatically identify an impostor\.

    - Nervous behavior and facial expressions are not decisive evidence\.

- **Genuine Experiences and Motive Twists**

    - Whether an accident happened and why someone was involved are designed separately\.

    - Twists reinterpret existing clues without overturning every established fact\.

    - A genuine survivor may have absurd, selfish, or embarrassing motives\.

    - Such motives add character depth without automatically disqualifying the injury experience\.

- **The Client’s Informed Decision**

    - The protagonist explains the verified candidate, supporting evidence, and exaggerated parts of the story\.

    - The client decides whether to arrange a date according to their personality\.

    - This introduces no new matching score or puzzle\.

    - A client declining the date does not mean the investigation failed\.

- **Date Scenes and Case Endings**

    - Acceptance leads to a date scene; rejection leads to a farewell or closing conversation\.

    - Optional discoveries may change dialogue or epilogue details\.

    - These scenes contain no additional gameplay challenges\.

    - Players can continue to the next case or replay the current one\.

### Risk and Feedback Systems

- **Formal Challenge Confirmation**

    - The selected statement and evidence appear together before submission\.

    - Only final confirmation triggers judgment and possible penalties\.

    - Final identification also requires confirmation to prevent accidental submission\.

- **Patience Meter and Incorrect Challenge Penalties**

    - The prototype starts each independent cross\-examination with 3 patience points\.

    - An incorrect formal challenge removes 1 point\.

    - Observation, ordinary questions, replay, and testimony loops are free\.

    - Temporarily leaving and returning to the same phase does not restore patience\.

    - Final identification is a separate phase with 3 patience points\.

    - Values will be adjusted through playtesting\.

- **Incorrect Deduction Feedback**

    - Feedback explains why the evidence does not contradict the selected statement\.

    - Relevant but insufficient evidence receives a specific response; unrelated evidence receives a brief general response\.

    - Feedback does not directly reveal the correct answer\.

    - Optional hints may appear after repeated mistakes\.

- **Successful Challenge Feedback**

    - The loop pauses, supported by sound, character reactions, and an evidence close\-up\.

    - The original statement and relevant fact are briefly compared\.

    - New dialogue explains what the player discovered\.

    - The end of the round is clearly announced\.

- **Bite Failure Scene**

    - When patience reaches zero, the candidate becomes furious at repeated incorrect accusations and bites the protagonist\.

    - Exaggerated movement, a bite sound, and the protagonist’s reaction deliver the dark humor\.

    - The game clearly states the failure reason and offers retry or main\-menu options\.

- **Checkpoints and Retry**

    - A checkpoint is created before each cross\-examination and final identification\.

    - Previously collected evidence, investigation progress, and completed conversations are preserved\.

    - Retrying restores the phase’s starting patience and story state\.

    - Earlier investigation does not need to be repeated, and previously viewed opening dialogue can be skipped\.

### Interaction and Presentation Systems

- **First\-Person Agency Interface**

    - The player sits on one side of the desk, with the candidate opposite them\.

    - The character area displays the portrait, body hotspots, and topic bubbles\.

    - The desk contains the commission file, evidence access, and approval tools\.

    - A small status area shows the current phase; cross\-examination also displays patience and statement numbers\.

    - **Order details are stored in the commission file’s secondary menu rather than permanently displayed\.**

    - New objectives appear briefly and then fade, leaving more room for characters and scenery\.

- **Visual Character and Object Interaction**

    - Information connected to a body part or object is accessed at that location\.

    - Abstract questions appear beside the character\.

    - A “Show Interaction Points” option prevents players from getting stuck searching for clickable areas\.

    - Hotspot states show reading progress, not whether someone is lying\.

- **Evidence Inspection and Accident Reconstruction Interactions**

    - Simple interactions include turning over photographs, magnifying details, comparing images, and arranging accident steps\.

    - They do not require quick reactions or permanently remove evidence after mistakes\.

    - Players can adjust incorrect attempts without losing patience\.

    - These interactions are used only when they improve observation or understanding\.

- **Centered Portraits and Single\-Line Dialogue**

    - The current conversation partner appears in the center, with their name and one line of dialogue below\.

    - Players advance manually as expressions and poses change\.

    - When the protagonist speaks, the nameplate changes while the candidate usually remains visible\.

    - Important facts are communicated through text and evidence, not expressions alone\.

    - Previously read content can be skipped quickly or replayed\.

- **Dialogue Choices and Evidence Interface**

    - Ordinary dialogue choices appear above the right side of the dialogue box\.

    - Cross\-examination provides Previous, Next, Question, and Present Evidence controls\.

    - The current statement shows its position, such as “2 / 4\.”

    - Closing the evidence or commission menu returns to the same statement\.

    - Unresolved testimony can be manually restarted with “Review Testimony Again\.”

- **Final Approval Ritual**

    - After a correct identification, players place the candidate’s file in the designated position\.

    - They use the agency’s denture stamp to leave a bite mark in the verification field\.

    - The stamp confirms the injury experience; it does not establish a romantic relationship\.

    - The client briefing follows\.

    - Both dragging and clicking are supported\. Speed and precision are not tested, and misplaced stamps do not reduce patience\.

- **Audio, Expressions, and Animation Feedback**

    - Observation, evidence collection, phase changes, and challenge results use distinct feedback\.

    - Physical traits contribute to animation, such as falling charcoal flakes, loose joints, or chattering dentures\.

    - Reactions support characterization and humor without treating nervousness as proof of lying\.

    - Players can mute audio and reduce screen shake\.

- **Language Switching and Progress Saving**

    - English is the default language, with Chinese available at any time\.

    - Switching languages preserves the scene, evidence, patience, and dialogue position\.

    - Statements and evidence retain the same logical meaning in both languages\.

    - Progress saves automatically after evidence collection, key conversations, phase changes, and case completion\.

    - Continue, Restart Case, and Clear Save options are provided, with confirmation before overwriting progress\.

